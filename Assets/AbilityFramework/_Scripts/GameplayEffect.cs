using System;
using System.Collections;
using UnityEngine;
using Object = System.Object;

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
[CreateAssetMenu(fileName = "New Gameplay Effect", menuName = "Gameplay/Effect")]
public class GameplayEffect : ScriptableObject, IDisposable
{
    public string effectName;
    public EEffectDurationType durationType;
    public float duration;
    public float period;
    public EModifierOperationType modifierType;
    [SerializeField] public GameplayAttribute targetAttribute;
    //public List<GameplayTag> requiredTags = new List<GameplayTag>();
    
    [SerializeReference, SubclassSelector] public IAttributeValueStrategy valueStrategy;
    
    public EEffectStackingType stackingType;
    private int maxStacks = 1;
    
    [SerializeField] private Object source;
    public Object Source
    {
        get => source;
        set => source = value;
    }
    
    private float modValue = 0;
    private GameplayAttributeComponent _owner;
    
    public bool _disposed;

    public float _elapsedTime;
    public float _periodElapsedTime;
    
    public void Initialize(
        string inEffectName,
        EEffectDurationType inDurationType,
        float inDuration,
        float inPeriod,
        EModifierOperationType inModifierType,
        GameplayAttribute inTargetAttribute,
        IAttributeValueStrategy inValueStrategy,
        EEffectStackingType inStackingType,
        GameplayAttributeComponent inOwner,
        object inSource = null)
    {
        effectName = inEffectName;
        durationType = inDurationType;
        duration = inDuration;
        period = inPeriod;
        modifierType = inModifierType;
        targetAttribute = inTargetAttribute;
        valueStrategy = inValueStrategy;
        stackingType = inStackingType;
        source = inSource;
        _owner = inOwner;
    }

    
    public IEnumerator ApplyEffect(GameplayAttributeComponent target)
    {
        if (!ValidateTags(target)) yield break;
        
        float modValue = valueStrategy.CalculateValue(targetAttribute, this);

        if (durationType == EEffectDurationType.Instant)
        {
            float newBase = CalculateModifiedValue(targetAttribute.BaseValue(), modValue, modifierType);;
            target.ModifyAttribute(targetAttribute, newBase, modifierType, true);
        }
        else
        {
            targetAttribute.AddModification(this, modValue);

            if (durationType == EEffectDurationType.Duration)
            {
                yield return new WaitForSeconds(duration);
                targetAttribute.RemoveModification(this);
            }
        }
    }
    
    public void ApplyInitialEffect(GameplayAttributeComponent target)
    {
        if (!ValidateTags(target)) return;
    
        float modValue = valueStrategy.CalculateValue(targetAttribute, this);
        Debug.Log($"Applying effect with value {modValue} to {targetAttribute.Name}");

        if (durationType == EEffectDurationType.Instant)
        {
            float newValue = CalculateModifiedValue(targetAttribute.BaseValue(), modValue, modifierType);
            Debug.Log($"Instant effect: new value will be {newValue}");
            target.ModifyAttribute(targetAttribute, newValue, modifierType, true);
        }
        else
        {
            Debug.Log($"Duration/Infinite effect: adding modification with value {modValue}");
            targetAttribute.AddModification(this, modValue);
        }
    }

    private bool ValidateTags(GameplayAttributeComponent target)
    {
        return true; // requiredTags.All(tag => target.HasTag(tag));
    }

    private void ModifyValue(GameplayAttributeComponent target, float value)
    {
        target.ModifyAttribute(targetAttribute, value, modifierType, durationType == EEffectDurationType.Instant);
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

    public void Dispose()
    {
        if (!_disposed)
        {/*
            if (targetAttribute != null)
            {
                targetAttribute.RemoveModification(this);
            }*/
            if (_owner != null)
            {
                _owner.RemoveEffect(this);
            }

            _disposed = true;
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }
}

public static class GameplayEffectFactory
{
    public static GameplayEffect CreateEffect(string inEffectName, EEffectDurationType inDurationType, 
        float inDuration, float inPeriod = 0, EModifierOperationType inModifierType = EModifierOperationType.Add,
        GameplayAttribute inTargetAttribute = null, IAttributeValueStrategy inValueStrategy = null, object inSource = null, GameplayAttributeComponent inOwner = null)
    {
        var effect = ScriptableObject.CreateInstance<GameplayEffect>();
        effect.Initialize(
            inEffectName,
            inDurationType,
            inDuration,
            inPeriod,
            inModifierType,
            inTargetAttribute,
            inValueStrategy,
            EEffectStackingType.None,
            inOwner,
            inSource
        );
        return effect;
    }
    
    public static GameplayEffect CreateEffect(string inEffectName, EEffectDurationType inDurationType,
        float inPeriod = 0, EModifierOperationType inModifierType = EModifierOperationType.Add,
        GameplayAttribute inTargetAttribute = null, IAttributeValueStrategy inValueStrategy = null)
    {
        var effect = ScriptableObject.CreateInstance<GameplayEffect>();
        effect.Initialize(
            inEffectName,
            inDurationType,
            0,
            inPeriod,
            inModifierType,
            inTargetAttribute,
            inValueStrategy,
            EEffectStackingType.None,
            null
        );
        return effect;
    }
    
    public static GameplayEffect CreateDamageEffect(float damageAmount, GameplayAttribute targetHealthAttribute)
    {
        var effect = ScriptableObject.CreateInstance<GameplayEffect>();
        var valueStrategy = new ConstantValueStrategy { value = -damageAmount };
        
        effect.Initialize(
            "Damage Effect",
            EEffectDurationType.Instant,
            0,
            0,
            EModifierOperationType.Add,
            targetHealthAttribute,
            valueStrategy,
            EEffectStackingType.None,
            null
        );
        return effect;
    }

    public static GameplayEffect CreateEquipmentEffect(
        string effectName,
        GameplayAttribute targetAttribute,
        GameplayAttribute sourceAttribute,
        float coefficient)
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
            targetAttribute,
            valueStrategy,
            EEffectStackingType.None,
            null
        );
        return effect;
    }
}