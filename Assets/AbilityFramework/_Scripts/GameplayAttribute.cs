using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct AttributeChangedEvent
{
    public readonly GameplayAttribute attribute;
    public readonly float oldValue { get; }
    public readonly float newValue { get; }
    
    public AttributeChangedEvent(GameplayAttribute inAttribute, float oldValue, float newValue)
    {
        attribute = inAttribute;
        this.oldValue = oldValue;
        this.newValue = newValue;
    }
}

[Serializable]
public class AttributeModification
{
    public GameplayEffect effect;
    public float value;

    public AttributeModification(GameplayEffect effect, float value)
    {
        this.effect = effect;
        this.value = value;
    }
}

[Serializable]
[CreateAssetMenu(fileName = "New Gameplay Attribute", menuName = "Gameplay/Attribute")]
public class GameplayAttribute : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private float _baseValue;
    [SerializeField] private float _currentValue;
    //[SerializeField] private Dictionary<GameplayTag, float> _taggedValues = new();
    [SerializeField] private List<AttributeModification> _activeModifications = new();
    [SerializeField] private List<GameplayEffectApplication> _activeEffectApplications = new();
    
    public event Action<AttributeChangedEvent> OnValueChanged;
    
    public GameplayAttribute(string name, float baseValue)
    {
        _name = name;
        _baseValue = baseValue;
        _currentValue = baseValue;
        //_taggedValues = new Dictionary<GameplayTag, float>();
        _activeModifications = new List<AttributeModification>();
        _activeEffectApplications = new List<GameplayEffectApplication>();
    }
    
    public void Initialize(string name, float baseValue)
    {
        _name = name;
        _baseValue = baseValue;
        _currentValue = baseValue;
        _activeModifications = new List<AttributeModification>();
        _activeEffectApplications = new List<GameplayEffectApplication>();
    }
    
    public string Name => _name;
    public float BaseValue => _baseValue;
    public float CurrentValue => _currentValue;

    public void SetValue(float incomingValue, bool modifyBase)
    {
        float oldValue = this._currentValue;
        float newValue = incomingValue;
        if (modifyBase)
        {
            this._baseValue = newValue;
            RecalculateCurrentValue();
        }
        else
            this._currentValue = newValue;
        
        OnValueChanged?.Invoke(new AttributeChangedEvent(this, oldValue, newValue));;
    }
    /*
    public void RecalculateCurrentValue()
    {
        float finalValue = _baseValue;
        Debug.Log($"Recalculating {_name} from base value {_baseValue}");
    
        foreach (var modification in _activeModifications)
        {
            float before = finalValue;
            finalValue = CalculateModifiedValue(finalValue, modification.value, modification.effect.modifierType);
            Debug.Log($"Applied modification: {before} -> {finalValue}");
        }
        SetValue(finalValue, false);
    }*/
    
    public void RecalculateCurrentValue()
    {
        Debug.Log($"ModsToApply {_activeEffectApplications.Count}");
        float finalValue = _baseValue;
        Debug.Log($"Recalculating {_name} from base value {_baseValue}");
    
        foreach (var modification in _activeEffectApplications)
        {
            float before = finalValue;
            finalValue = CalculateModifiedValue(finalValue, modification.GetComputedValue(), modification.modifierOperation);
            Debug.Log($"Applied modification: {before} -> {finalValue} for {_name}");
        }
        SetValue(finalValue, false);
    }
    /*
    public void RecalculateCurrentValue(
        Dictionary<GameplayAttribute, List<(GameplayEffect effect, float modification)>> modifications,
        Func<float, float, EModifierOperationType, float> calculateModifiedValue)
    {
        float finalValue = _baseValue;

        if (modifications.TryGetValue(this, out var mods))
        {
            foreach (var (effect, modValue) in mods)
            {
                finalValue = calculateModifiedValue(finalValue, modValue, effect.modifierType);
            }
        }
        
        SetValue(finalValue, false);
    }*/
    
    public float CalculateModifiedValue(float currentValue, float modValue, EModifierOperationType modType)
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

    public void AddModification(GameplayEffect effect, float value)
    {
        Debug.Log($"Adding modification to {_name}: {value}");
        _activeModifications.Add(new AttributeModification(effect, value));
        RecalculateCurrentValue();
    }
    
    public void AddModification(GameplayEffectApplication inModification)
    {
        Debug.Log($"Adding modification to {_name}: {inModification.GetComputedValue()}");
        _activeEffectApplications.Add(inModification);
        RecalculateCurrentValue();
    }

    public void RemoveModification(GameplayEffect effect)
    {
        var modifiersToRemove = effect.GetModifiers();
        foreach (var modifierToRemove in modifiersToRemove)
        {
            _activeEffectApplications.Remove(modifierToRemove);
        }
        _activeModifications.RemoveAll(x => x.effect == effect);
        RecalculateCurrentValue();
    }

    public void RemoveModification(GameplayEffectApplication effect)
    {
        _activeEffectApplications.Remove(effect);
        RecalculateCurrentValue();
    }
    /*
    public float GetValueForTag(GameplayTag tag)
    {
        return _taggedValues.TryGetValue(tag, out var value) ? value : _currentValue;
    }*/
}