using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace LM.AbilitySystem
{
    public enum EModifierOperationType
    {
        Add,
        Multiply,
        Divide,
        Percent,
        Override
    }

    public enum EEffectDurationType { Instant, Infinite, Duration }

    public enum EEffectStackingType { None, Duration, Intensity }

    [Serializable]
    public class GameplayEffectApplication
    {
        public GameplayAttribute targetAttribute;
        public EModifierOperationType modifierOperation;
        [SerializeReference, SubclassSelector] public IAttributeMagnitudeStrategy valueStrategy;

        [NonSerialized] private float _computedValue;

        public float ComputeValue(GameplayAttributeComponent source, GameplayAttributeComponent target = null)
        {
            if (valueStrategy != null)
            {
                _computedValue = valueStrategy.CalculateMagnitude(source);
            }
            else
            {
                _computedValue = 0f;
            }
            return _computedValue;
        }

        public float GetComputedValue() => _computedValue;

        public GameplayEffectApplication(GameplayAttribute inTargetAttribute,
            EModifierOperationType inModifierOperation, IAttributeMagnitudeStrategy inValueStrategy)
        {
            targetAttribute = inTargetAttribute;
            modifierOperation = inModifierOperation;
            valueStrategy = inValueStrategy;
        }

        private float CalculateModifiedValue(float currentValue, float modValue, EModifierOperationType modType)
        {
            return modType switch
            {
                EModifierOperationType.Add => currentValue + modValue,
                EModifierOperationType.Multiply => currentValue * modValue,
                EModifierOperationType.Divide => currentValue / modValue,
                EModifierOperationType.Percent => currentValue * (modValue / 100f),
                EModifierOperationType.Override => modValue,
                _ => currentValue
            };
        }

        public GameplayEffectApplication CloneApplication()
        {
            var clonedApplication = new GameplayEffectApplication(
                targetAttribute,
                modifierOperation,
                valueStrategy
            );

            if (valueStrategy != null)
            {
                if (this.valueStrategy is ConstantValueStrategy constantValueStrategy)
                {
                    clonedApplication.valueStrategy = new ConstantValueStrategy { value = constantValueStrategy.value };
                }
                else if (this.valueStrategy is CurveValueStrategy curveValueStrategy)
                {
                    clonedApplication.valueStrategy = new CurveValueStrategy
                    {
                        curve = new AnimationCurve(curveValueStrategy.curve.keys),
                        timeScale = curveValueStrategy.timeScale
                    };
                }
                else if (this.valueStrategy is AttributeBasedValueStrategy attributeBasedValueStrategy)
                {
                    clonedApplication.valueStrategy = new AttributeBasedValueStrategy
                    {
                        sourceAttribute = attributeBasedValueStrategy.sourceAttribute,
                        _coefficient = attributeBasedValueStrategy._coefficient
                    };
                }
            }

            return clonedApplication;
        }
    }

    [Serializable]
    [CreateAssetMenu(fileName = "New Gameplay Effect", menuName = "Gameplay/Effect")]
    public class GameplayEffect : ScriptableObject, IDisposable
    {
        public string effectName;
        public EEffectDurationType durationType;
        public float duration;
        public float period;
        public EModifierOperationType modifierType;
        //public List<GameplayTag> requiredTags = new List<GameplayTag>();

        [SerializeReference, SubclassSelector] public IAttributeMagnitudeStrategy valueStrategy;
        [SerializeField] public List<GameplayEffectApplication> applications = new();
        [SerializeReference, SubclassSelector] public List<IGameplayEffectExecution> executions = new();

        public event Action<GameplayEffect> OnApply;
        public event Action<GameplayEffect> OnRemove;

        public EEffectStackingType stackingType;
        private int maxStacks = 1;

        [SerializeField] private Object source;

        public Object Source
        {
            get => source;
            set => source = value;
        }

        private GameplayAttributeComponent _owner;
        public GameplayAttributeComponent Owner
        {
            get => _owner;
            set => _owner = value;
        }

        private float _elapsedTime;

        public void Initialize(
            string inEffectName,
            EEffectDurationType inDurationType,
            float inDuration,
            float inPeriod,
            EModifierOperationType inModifierType,
            IAttributeMagnitudeStrategy inValueStrategy,
            EEffectStackingType inStackingType,
            GameplayAttributeComponent inOwner,
            object inSource = null)
        {
            effectName = inEffectName;
            durationType = inDurationType;
            duration = inDuration;
            period = inPeriod;
            modifierType = inModifierType;
            valueStrategy = inValueStrategy;
            stackingType = inStackingType;
            source = inSource;
            _owner = inOwner;
        }

        private bool ValidateTags(GameplayAttributeComponent target)
        {
            return true; // requiredTags.All(tag => target.HasTag(tag));
        }

        public List<GameplayEffectApplication> GetModifiers()
        {
            if (applications == null) return new List<GameplayEffectApplication>();

            var modifiers = new List<GameplayEffectApplication>();
            foreach (var modifier in applications)
            {
                modifiers.Add(modifier);
            }
            return modifiers;
        }

        public IEnumerator Apply()
        {
            OnApply?.Invoke(this);
            if (durationType != EEffectDurationType.Instant)
            {
                this._elapsedTime = 0;

                while (_elapsedTime < duration || durationType == EEffectDurationType.Infinite)
                {
                    yield return null;
                    this._elapsedTime += Time.deltaTime;
                    if (period > 0 && this._elapsedTime % period < Time.deltaTime)
                    {
                        OnApply?.Invoke(this);
                    }
                }
                Dispose();
                yield break;
            }
            Dispose();
        }

        public GameplayEffect Clone()
        {
            var clone = Instantiate(this);
            clone.applications = new List<GameplayEffectApplication>();
            foreach (var application in applications)
            {
                clone.applications.Add(application.CloneApplication());
            }
            return clone;
        }

        public void Dispose()
        {
            OnRemove?.Invoke(this);
        }
    }
}