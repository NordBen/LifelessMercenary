using System;
using System.Collections.Generic;
using UnityEngine;

namespace LM.AbilitySystem
{
    public interface IAttributeMagnitudeStrategy
    {
        float CalculateMagnitude(GameplayAttributeComponent target = null);
    }

    [Serializable]
    public class ConstantValueStrategy : IAttributeMagnitudeStrategy
    {
        public float value;

        public float CalculateMagnitude(GameplayAttributeComponent target)
        {
            return value;
        }
    }

    [Serializable]
    public class CurveValueStrategy : IAttributeMagnitudeStrategy
    {
        public AnimationCurve curve;
        public float timeScale = 1f;

        public float CalculateMagnitude(GameplayAttributeComponent target)
        {
            return curve.Evaluate(Time.time * timeScale);
        }
    }

    [Serializable]
    public class AttributeBasedValueStrategy : IAttributeMagnitudeStrategy
    {
        public GameplayAttribute sourceAttribute;
        public float _coefficient;

        public float CalculateMagnitude(GameplayAttributeComponent target)
        {
            if (sourceAttribute == null)
            {
                Debug.LogError($"sourceAttribute is null for {this} val: {sourceAttribute}");
            }
            else
            {
                if (target == null)
                {
                    Debug.LogError($"target is null for {this} val: {target}");
                }
                else
                {
                    Debug.Log($"{sourceAttribute.GetInstanceID()} will become {target.GetAttribute(sourceAttribute.Name).GetInstanceID()}");
                }
            }
            sourceAttribute = target != null ? target.GetAttribute(sourceAttribute.Name) : sourceAttribute;
            return sourceAttribute.CurrentValue * _coefficient;
        }
    }

    public class GameplayEffectContext
    {
        GameObject Source { get; set; }
        GameObject Target { get; set; }
        public GameplayAttributeComponent SourceAttributeComponent { get; set; }
        public GameplayAttributeComponent TargetAttributeComponent { get; set; }
        public GameplayEffect Effect { get; set; }

        public Dictionary<string, object> ContextTags = new();

        public GameplayEffectContext(GameObject source, GameObject target, GameplayEffect effect)
        {
            Source = source;
            Target = target;
            Effect = effect;

            if (source != null)
                SourceAttributeComponent = source.GetComponent<GameplayAttributeComponent>();

            if (target != null)
                TargetAttributeComponent = target.GetComponent<GameplayAttributeComponent>();
        }

        public T GetContextData<T>(string key) where T : class
        {
            if (ContextTags.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return null;
        }

        public void SetContextData(string key, object value)
        {
            ContextTags[key] = value;
        }
    }

    public interface IModifierMagnitudeStrategy
    {
        float ModifierMagnitude(GameplayEffectContext effectContext);
    }

    [Serializable]
    public class ModifierMagnitudeStrategy : IAttributeMagnitudeStrategy
    {
        public IModifierMagnitudeStrategy modMagCalculation;
        private GameplayEffectContext context;

        [SerializeField] private bool captureLevelForSource = false;
        [SerializeField] private bool captureLevelForTarget = false;

        public virtual float CalculateMagnitude(GameplayAttributeComponent target)
        {
            return modMagCalculation.ModifierMagnitude(context);
        }

        protected float GetCapturedAttributeValue(GameplayEffectContext context, string attributeName,
            bool fromSource = true)
        {
            var component = fromSource ? context.SourceAttributeComponent : context.TargetAttributeComponent;
            if (component == null) return 0f;

            var attribute = component.GetAttribute(attributeName);
            return attribute?.CurrentValue ?? 0f;
        }

        public void SetContext(GameplayEffectContext context)
        {
            this.context = context;
        }
    }

    [Serializable]
    public class DamageCalculationStrategy : IModifierMagnitudeStrategy
    {
        public float ModifierMagnitude(GameplayEffectContext effectContext)
        {
            float baseDamage = 0f;
            /*
            var combatManager = effectContext.Source.GetComponent<CombatManager>();
            if (combatManager != null && combatManager.weapon != null)
            {
                baseDamage = combatManager.weapon.weaponData.damage;
            }

            var sourceAttributes = effectContext.SourceAttributeComponent;
            if (sourceAttributes != null)
            {
                var strength = sourceAttributes.GetAttribute("Strength");
                if (strength != null)
                {
                    baseDamage += strength.CurrentValue;
                }
            }*/
            return baseDamage;
        }
    }
/*
[Serializable]
public abstract class GameplayEffectModifierMagnitudeCalculation : IAttributeMagnitudeStrategy
{
    protected List<FAttributeCaptureDef> CapturedAttributes = new();
    protected Dictionary<GameplayAttribute, float> CapturedValues = new();

    public abstract float CalculateMagnitude(GameplayEffect effect, GameplayAttributeComponent source, GameplayAttributeComponent target);

    public float CalculateValue(GameplayAttribute attribute, GameplayEffectApplication modification)
    {
        GameplayAttributeComponent source = null;
        GameplayAttributeComponent target = null;

        if (effect.Source is GameObject sourceGO)
            source = sourceGO.GetComponent<GameplayAttributeComponent>();

        if (effect.Owner != null)
        {
            target = effect.Owner;
        }

        return CalculateMagnitude(effect, source, target);
    }

    protected void CaptureAttributes(GameplayAttributeComponent source, GameplayAttributeComponent target)
    {
        CapturedValues.Clear();

        foreach (var capturedAttribute in CapturedAttributes)
        {
            var component = capturedAttribute.IsSource ? source : target;
            if (component == null) continue;

            var attribute = component.GetAttribute(capturedAttribute.attribute.Name);
            if (attribute != null)
            {
                CapturedValues[capturedAttribute.attribute] = attribute.CurrentValue();
            }
        }
    }
}

[Serializable]
public class WeaponDamageModMag : GameplayEffectModifierMagnitudeCalculation
{
    [SerializeField] private float strengthScaling = 2.5f;
    [SerializeField] private float agilityScaling = 1.5f;

    public WeaponDamageModMag()
    {
        CapturedAttributes.Add(new FAttributeCaptureDef
        {
            attribute = Resources.Load<GameplayAttribute>("Attributes/Stats/Strength"),
            IsSource = true,
            bSnapshot = false
        });

        CapturedAttributes.Add(new FAttributeCaptureDef
        {
            attribute = Resources.Load<GameplayAttribute>("Attributes/Stats/Agility"),
            IsSource = true,
            bSnapshot = false
        });
    }

    public override float CalculateMagnitude(GameplayEffect effect, GameplayAttributeComponent source, GameplayAttributeComponent target)
    {
        CaptureAttributes(source, target);

        float strength = 0, agility = 0;

        if (CapturedValues.TryGetValue(Resources.Load<GameplayAttribute>("Attributes/Stats/Strength"), out float stengthValue))
        {
            strength = stengthValue;
        }

        if (CapturedValues.TryGetValue(Resources.Load<GameplayAttribute>("Attributes/Stats/Agility"), out float agilityValue))
        {
            agility = agilityValue;
        }

        float weaponDamage = 0;
        if (source != null)
        {
            var equipmentComponent = source.GetComponent<EquipmentManager>();
            if (equipmentComponent != null && equipmentComponent.GetEquippedItem(EEquipSlot.Weapon) != null)
            {
                var weapon = equipmentComponent.GetEquippedItem(EEquipSlot.Weapon) as Weapon;

                float physicalDamage = weapon.damage;
                float fireDamage = weapon.damage *= 1.2f;
                float lightningDamage = 0;
                float iceDamage = 0;
                float poisonDamage = 0;
                weaponDamage = physicalDamage + fireDamage + lightningDamage + iceDamage + poisonDamage;
            }
        }

        float scaledDamage = (strengthScaling * strength) + (agilityScaling * agility);

        float totalDamage = weaponDamage + scaledDamage;

        Debug.Log($"Calculated weapon damage: {totalDamage} (Base: {weaponDamage}, Str: {strength}, Agi: {agility})");

        return totalDamage;
    }
}*/
}