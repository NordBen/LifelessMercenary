using System;
using System.Collections.Generic;
using LM.Inventory;
using UnityEngine;

namespace LM.AbilitySystem
{
    [Serializable]
    public struct FAttributeCaptureDef
    {
        public GameplayAttribute attribute;
        public bool bSnapshot;
        public bool IsSource;

        private float? SnapshotValue;

        public FAttributeCaptureDef(GameplayAttribute attribute, bool bSnapshot, bool IsSource)
        {
            this.attribute = attribute;
            this.IsSource = IsSource;
            this.bSnapshot = bSnapshot;
            SnapshotValue = null;
        }

        public float GetValue(GameplayAttributeComponent source, GameplayAttributeComponent target)
        {
            if (bSnapshot && SnapshotValue.HasValue)
                return SnapshotValue.Value;

            var component = IsSource ? source : target;
            if (component == null) return 0f;

            var attribute = component.GetAttribute(this.attribute.Name);
            float value = attribute?.CurrentValue ?? 0f;

            if (bSnapshot && !SnapshotValue.HasValue)
                SnapshotValue = value;

            return value;
        }
    }

    public interface IGameplayEffectExecution
    {
        void Execute(GameplayEffect effect, GameplayAttributeComponent target);
    }

    [Serializable]
    public abstract class GameplayEffectExecutionCalculation : ScriptableObject
    {
        protected List<FAttributeCaptureDef> RelevantAttributesToCapture = new();

        public virtual void Execute(GameplayEffect effect, GameplayAttributeComponent source,
            GameplayAttributeComponent target, out Dictionary<GameplayAttribute, float> OutModifications)
        {
            Dictionary<GameplayAttribute, float> capturedAttributes = CaptureAttributes(source, target);

            OutModifications = new Dictionary<GameplayAttribute, float>();
            CalculateExecution(effect, source, target, capturedAttributes, ref OutModifications);
        }

        protected abstract void CalculateExecution(GameplayEffect effect, GameplayAttributeComponent source,
            GameplayAttributeComponent target, Dictionary<GameplayAttribute, float> capturedAttributes,
            ref Dictionary<GameplayAttribute, float> outModifications);

        private Dictionary<GameplayAttribute, float> CaptureAttributes(GameplayAttributeComponent source,
            GameplayAttributeComponent target)
        {
            var result = new Dictionary<GameplayAttribute, float>();

            foreach (var captureDef in RelevantAttributesToCapture)
            {
                GameplayAttributeComponent component = captureDef.IsSource ? source : target;

                if (component != null)
                {
                    GameplayAttribute attribute = component.GetAttribute(captureDef.attribute.Name);

                    if (attribute != null)
                    {
                        result[captureDef.attribute] = attribute.CurrentValue;
                    }
                }
            }

            return result;
        }
    }

    public class DamageExecution : GameplayEffectExecutionCalculation
    {
        private static GameplayAttribute HealthAttribute;
        private static GameplayAttribute ArmorAttribute;
        private static GameplayAttribute DamageAttribute;
        private static GameplayAttribute CritChanceAttribute;
        private static GameplayAttribute CritMultiAttribute;
        private static GameplayAttribute DodgeChanceAttribute;

        public DamageExecution()
        {
            if (HealthAttribute == null)
            {
                HealthAttribute = Resources.Load<GameplayAttribute>("AbilityFramework/Attributes/Health");
                ArmorAttribute = Resources.Load<GameplayAttribute>("AbilityFramework/Attributes/Armor");
                DamageAttribute = Resources.Load<GameplayAttribute>("AbilityFramework/Attributes/Damage");
                CritChanceAttribute = Resources.Load<GameplayAttribute>("AbilityFramework/Attributes/CritChance");
                CritChanceAttribute = Resources.Load<GameplayAttribute>("AbilityFramework/Attributes/CritMulti");
                DodgeChanceAttribute = Resources.Load<GameplayAttribute>("AbilityFramework/Attributes/DodgeChance");
            }

            RelevantAttributesToCapture.Add(new FAttributeCaptureDef(HealthAttribute, false, false));
            RelevantAttributesToCapture.Add(new FAttributeCaptureDef(ArmorAttribute, false, false));
            RelevantAttributesToCapture.Add(new FAttributeCaptureDef(DamageAttribute, false, true));
            RelevantAttributesToCapture.Add(new FAttributeCaptureDef(CritChanceAttribute, false, true));
            RelevantAttributesToCapture.Add(new FAttributeCaptureDef(CritMultiAttribute, false, true));
            RelevantAttributesToCapture.Add(new FAttributeCaptureDef(DodgeChanceAttribute, false, false));
        }

        protected override void CalculateExecution(GameplayEffect effect, GameplayAttributeComponent source,
            GameplayAttributeComponent target,
            Dictionary<GameplayAttribute, float> capturedAttributes,
            ref Dictionary<GameplayAttribute, float> outModifications)
        {
            float sourceDamage = capturedAttributes.TryGetValue(DamageAttribute, out float damage) ? damage : 0;
            float targetArmor = capturedAttributes.TryGetValue(ArmorAttribute, out float armorValue) ? armorValue : 0;
            float critChance = capturedAttributes.TryGetValue(CritChanceAttribute, out float critChanceValue)
                ? critChanceValue
                : 0;
            float sourceCritMulti = capturedAttributes.TryGetValue(DodgeChanceAttribute, out float sourceCritMultiValue)
                ? sourceCritMultiValue
                : 1.5f;
            float dodgeChance = capturedAttributes.TryGetValue(DodgeChanceAttribute, out float dodgeChanceValue)
                ? dodgeChanceValue
                : 0;

            var equipmentComponent = source.GetComponent<EquipmentManager>();
            if (equipmentComponent != null)
            {
                Weapon equippedWeapon = equipmentComponent.GetEquippedItem(EEquipSlot.Weapon) as Weapon;
                if (equippedWeapon != null)
                {
                    sourceDamage += equippedWeapon.damage;
                }
            }

            bool isCritical = UnityEngine.Random.Range(0f, 100f) < critChance;
            bool isDodged = UnityEngine.Random.Range(0f, 100f) < dodgeChance;
            bool isBlocked = target.GetComponent<CombatManager>().isBlocking;

            float finalDamage = sourceDamage;

            if (isCritical)
            {
                finalDamage *= sourceCritMulti;
                Debug.Log("Critical Hit!");
            }

            if (isBlocked)
            {
                finalDamage *= 0.1f;
                Debug.Log("Blocked!");
            }

            if (isDodged)
            {
                finalDamage = 0;
                Debug.Log("Dodged!");
            }

            finalDamage = Mathf.Max(0, finalDamage - targetArmor);
            outModifications[HealthAttribute] = -finalDamage;
        }
    }

    [Serializable]
    public class WeaponDamageEffectExecution : GameplayEffectExecutionCalculation
    {
        protected override void CalculateExecution(GameplayEffect effect, GameplayAttributeComponent source,
            GameplayAttributeComponent target,
            Dictionary<GameplayAttribute, float> capturedAttributes,
            ref Dictionary<GameplayAttribute, float> outModifications)
        {
            GameplayAttribute damageAttribute = source.GetAttribute("Damage");

            if (damageAttribute != null)
            {
                var equipmentComponent = source.GetComponent<EquipmentManager>();
                float attackDamage = 0;

                if (equipmentComponent != null)
                {
                    var equippedWeapon = equipmentComponent.GetEquippedItem(EEquipSlot.Weapon) as Weapon;
                    if (equippedWeapon != null)
                    {
                        attackDamage = equippedWeapon.damage;
                    }
                }

                outModifications[damageAttribute] = attackDamage;
            }
        }
    }

    [Serializable]
    public class DamageExecutionTest : IGameplayEffectExecution
    {
        public float damagePercent;

        public void Execute(GameplayEffect effect, GameplayAttributeComponent target)
        {
            var healthAttribute = target.GetAttribute("Health");
            if (healthAttribute != null)
            {
                var damage = healthAttribute.CurrentValue * (damagePercent * 0.01f);
                target.ModifyAttribute(healthAttribute, healthAttribute.CurrentValue - damage,
                    EModifierOperationType.Override, false);
            }
        }
    }
}