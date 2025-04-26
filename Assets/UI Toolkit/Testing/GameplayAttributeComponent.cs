using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

public class GameplayAttributeComponent : MonoBehaviour
{
    [CanBeNull] private Dictionary<GameplayAttribute, List<(GameplayEffect effect, float modification)>> _attributeModifications = new(); // new
    
    [SerializeField] public List<GameplayAttribute> attributesToAdd = new();
    private Dictionary<string, GameplayAttribute> _attributes = new();

    private event Action<GameplayAttribute> OnPreAttributeModified;
    private event Action<GameplayAttribute> OnPostAttributeModified;
    
    [SerializeField] private List<GameplayEffect> _activeEffects = new();

    private void Awake()
    {
        OnPreAttributeModified += PreAttributeModificaton;
        OnPostAttributeModified += PostAttributeModificaton;

        InitializeRuntimeAttributes();
    }

    private void OnDisable()
    {
        OnPreAttributeModified -= PreAttributeModificaton;
        OnPostAttributeModified -= PostAttributeModificaton;
    }

    private void InitializeRuntimeAttributes()
    {
        foreach (var attribute in attributesToAdd)
        {
            var attributeInstance = ScriptableObject.CreateInstance<GameplayAttribute>();
            attributeInstance.Initialize(attribute.Name, attribute.BaseValue());
            _attributes[attributeInstance.Name] = attributeInstance;
        }
    }
    
    private GameplayAttribute GetRuntimeAttribute(GameplayAttribute attributeAsset)
    {
        return _attributes.TryGetValue(attributeAsset.Name, out var runtimeAttr) 
            ? runtimeAttr 
            : attributeAsset;
    }

    
    private void Update()
    {
        foreach (var pair in _attributes)
        {
            Debug.Log(pair.Value.CurrentValue());
        }
    }
    
    private class ActiveEffectData
    {
        public GameplayEffect Effect;
        public float ElapsedTime;
        public float PeriodElapsedTime;

        public ActiveEffectData(GameplayEffect effect)
        {
            Effect = effect;
            ElapsedTime = 0f;
            PeriodElapsedTime = 0f;
        }
    }

    private List<ActiveEffectData> _activeEffectData = new();

    private void UpdateEffectModification(GameplayEffect effect, float modValue)
    {
        if (!_attributeModifications.ContainsKey(effect.targetAttribute))
        {
            _attributeModifications[effect.targetAttribute] = new List<(GameplayEffect, float)>();
        }
    
        var modifications = _attributeModifications[effect.targetAttribute];
        var existingMod = modifications.FindIndex(m => m.effect == effect);

        // Changed condition: if existingMod is -1, we add new modification
        if (existingMod == -1)  // Changed from != to ==
        {
            modifications.Add((effect, modValue));
        }
        else
        {
            // Update existing modification
            modifications[existingMod] = (effect, modValue);
        }

        RecalculateAttributeValue(effect.targetAttribute);
    }

    private void RecalculateAttributeValue(GameplayAttribute attribute)
    {
        float baseValue = attribute.BaseValue();
        float finalValue = baseValue;

        if (_attributeModifications.TryGetValue(attribute, out var modifications))
        {
            foreach (var (effect, modValue) in modifications)
            {
                finalValue = CalculateModifiedValue(finalValue, modValue, effect.modifierType);
            }
        }
        
        attribute.SetValue(finalValue, false);
    }

    public virtual void PreAttributeModificaton(GameplayAttribute attribute)
    {
        Debug.Log("PreModification:");
    }
    
    public virtual void PostAttributeModificaton(GameplayAttribute attribute)
    {
        Debug.Log("PostModification");
    }

    public void ModifyAttribute(GameplayAttribute attribute, float value, EModifierOperationType modType, bool modifyBase)
    {
        if (_attributes.TryGetValue(attribute.Name, out var runtimeAttribute))
        {
            OnPreAttributeModified?.Invoke(attribute);
            runtimeAttribute.SetValue(value, modifyBase);
            OnPostAttributeModified?.Invoke(attribute);
        }
    }
    
    public void ApplyEffect(GameplayEffect effect)
    {
        var instancedEffect = Instantiate(effect);
        _activeEffects.Add(instancedEffect);

        if (_attributes.TryGetValue(effect.targetAttribute.Name, out var runtimeAttribute))
        {
            Debug.Log(effect.targetAttribute.Name);
            instancedEffect.targetAttribute = runtimeAttribute;
        }
        
        if (instancedEffect.durationType == EEffectDurationType.Instant)
        {
            instancedEffect.ApplyInitialEffect(this);
        }
        else
        {
            instancedEffect.ApplyInitialEffect(this);
            StartCoroutine(HandleDurationEffect(instancedEffect));
        }
    }
    
    private IEnumerator HandleDurationEffect(GameplayEffect effect)
    {
        float elapsedTime = 0;
        float periodTime = 0;
        
        bool hasPeriodicEffect = effect.period > 0;

        while (effect.durationType == EEffectDurationType.Infinite || 
               elapsedTime < effect.duration)
        {
            elapsedTime += Time.deltaTime;

            if (hasPeriodicEffect)
            {
                periodTime += Time.deltaTime;
                if (periodTime >= effect.period)
                {
                    float modValue = effect.valueStrategy.CalculateValue(effect.targetAttribute, effect);
                    effect.targetAttribute.AddModification(effect, modValue);
                    periodTime = 0;
                }
            }
            yield return null;
        }
        effect.targetAttribute.RemoveModification(effect);
        RemoveEffect(effect);
    }

    
    public void RemoveEffect(GameplayEffect effect)
    {
        if (_activeEffects.Contains(effect))
        {
            _activeEffects.Remove(effect);
            
            var attributeInstance = GetRuntimeAttribute(effect.targetAttribute);
            if (_attributeModifications.TryGetValue(attributeInstance, out var modifications))
            {
                modifications.RemoveAll(m => m.effect == effect);
                RecalculateAttributeValue(effect.targetAttribute);
            }
            
            //Destroy(effect);
        }
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
}