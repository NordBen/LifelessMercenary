using System.Collections.Generic;
using UnityEngine;

namespace LM.AbilitySystem
{
    public static class EffectFactory
    {
        public static GameplayEffect CreateEffect(
            string inEffectName, EEffectDurationType inDurationType,
            float inDuration, List<GameplayEffectApplication> applications, float inPeriod = 0,
            object inSource = null, GameplayAttributeComponent inOwner = null)
        {
            var effect = ScriptableObject.CreateInstance<GameplayEffect>();
            effect.effectName = inEffectName;
            effect.durationType = inDurationType;
            effect.duration = inDuration;
            effect.period = inPeriod;
            effect.applications = applications ?? new List<GameplayEffectApplication>();
            return effect;
        }

        public static GameplayEffect CreateAttributeUpgradeEffect(GameplayAttribute targetAttribute, float upgradeAmount)
        {
            var effect = ScriptableObject.CreateInstance<GameplayEffect>();
            var application = new GameplayEffectApplication
                (targetAttribute, EModifierOperationType.Add, new ConstantValueStrategy() { value = upgradeAmount });

            effect.effectName = "UpgradeAttribute" + targetAttribute.Name;
            effect.durationType = EEffectDurationType.Instant;
            effect.applications = new List<GameplayEffectApplication> { application };
            return effect;
        }

        public static GameplayEffect CreateEquipmentEffect(
            string effectName, GameplayAttribute targetAttribute, GameplayAttribute sourceAttribute, float coefficient)
        {
            var effect = ScriptableObject.CreateInstance<GameplayEffect>();
            var valueStrategy = new AttributeBasedValueStrategy
            {
                sourceAttribute = sourceAttribute,
                _coefficient = coefficient
            };

            effect.Initialize(
                effectName,
                EEffectDurationType.Infinite,
                0,
                0,
                EModifierOperationType.Add,
                valueStrategy,
                EEffectStackingType.None,
                null
            );
            return effect;
        }
    }
}