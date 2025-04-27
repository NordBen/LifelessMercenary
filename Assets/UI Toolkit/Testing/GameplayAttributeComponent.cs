using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
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
    
    public GameplayAttribute GetRuntimeAttribute(GameplayAttribute attributeAsset)
    {
        return _attributes.TryGetValue(attributeAsset.Name, out var runtimeAttr) 
            ? runtimeAttr 
            : attributeAsset;
    }

    public GameplayAttribute GetAttribute(string attributeName)
    {
        return _attributes.TryGetValue(attributeName, out var attribute) ? attribute : null;
    }
    
    private void Update()
    {
        foreach (var pair in _attributes)
        {
            Debug.Log(pair.Value.CurrentValue());
        }
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

    protected virtual void PreAttributeModificaton(GameplayAttribute attribute)
    {
        Debug.Log("PreModification:");
    }
    
    protected virtual void PostAttributeModificaton(GameplayAttribute attribute)
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
    
    public void ApplyEffect(GameplayEffect effect, bool isPreCreated = false)
    {
        if (effect == null)
        {
            Debug.LogWarning("Attempting to apply null effect");
            return;
        }
        
        Debug.Log($"[{Time.frameCount}] Applying effect: {effect.effectName} (PreCreated: {isPreCreated})");

        //Debug.Log($"Applying effect: {effect.effectName} to {gameObject.name}");
        GameplayEffect instancedEffect = null;
        if (isPreCreated)
        {
            instancedEffect = effect;
        }
        else
        {
            instancedEffect = Instantiate(effect);
        }
        
        instancedEffect.Initialize(instancedEffect.effectName, effect.durationType, effect.duration, effect.period, effect.modifierType, effect.targetAttribute, effect.valueStrategy, effect.stackingType, this);
        Debug.Log($"[{Time.frameCount}] Adding to _activeEffects: {instancedEffect.effectName}");
        _activeEffects.Add(instancedEffect);

        if (_attributes.TryGetValue(effect.targetAttribute.Name, out var runtimeAttribute))
        {
            Debug.Log($"Found runtime attribute: {effect.targetAttribute.Name}");
            Debug.Log(effect.targetAttribute.Name);
            instancedEffect.targetAttribute = runtimeAttribute;
            if (instancedEffect.durationType == EEffectDurationType.Instant)
            {
                instancedEffect.ApplyInitialEffect(this);
                RemoveEffect(instancedEffect);
            }
            else
            {
                instancedEffect.ApplyInitialEffect(this);
                StartCoroutine(HandleDurationEffect(instancedEffect));
            }

        }
        else
        {
            Debug.LogWarning($"Could not find runtime attribute: {effect.targetAttribute.Name}");
            return;
        }
        /*
        if (instancedEffect.durationType == EEffectDurationType.Instant)
        {
            instancedEffect.ApplyInitialEffect(this);
        }
        else
        {
            instancedEffect.ApplyInitialEffect(this);
            StartCoroutine(HandleDurationEffect(instancedEffect));
        }*/
    }

    private IEnumerator HandleDurationEffect(GameplayEffect effect)
    {
        float elapsedTime = 0;
        float periodTime = 0;
        /*
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
        RemoveEffect(effect);*/

        while (!effect._disposed && 
               (effect.durationType == EEffectDurationType.Infinite || 
                elapsedTime < effect.duration))
        {
            elapsedTime += Time.deltaTime;

            if (!effect._disposed && effect.period > 0)
            {
                periodTime += Time.deltaTime;
                if (periodTime >= effect.period)
                {
                    effect.ApplyInitialEffect(this);
                    periodTime = 0;
                }
            }

            yield return null;
        }

        if (!effect._disposed)
        {
            RemoveEffect(effect);
        }

    }
    
    public void RemoveEffect(GameplayEffect effect)
    {
        if (effect == null) return;
        Debug.Log($"Removing effect: {effect.effectName}");
        /*
        if (_activeEffects.Contains(effect))
        {
            _activeEffects.Remove(effect);
            /*
            var attributeInstance = GetRuntimeAttribute(effect.targetAttribute);
            if (_attributeModifications.TryGetValue(attributeInstance, out var modifications))
            {
                modifications.RemoveAll(m => m.effect == effect);
                RecalculateAttributeValue(effect.targetAttribute);
            }*/
            /*
            var attributeInstance = GetRuntimeAttribute(effect.targetAttribute);
            if (attributeInstance != null)
            {
                Debug.Log($"Removing effect for real: {effect.effectName} from {attributeInstance.Name}");
                attributeInstance.RemoveModification(effect);
                attributeInstance.RecalculateCurrentValue();
            }*
            effect.targetAttribute.RemoveModification(effect);
            effect.targetAttribute.RecalculateCurrentValue();*/
            
           // effect.Dispose();
            //Destroy(effect);
        //}
        
        
    
        // Then remove the modification from the attribute
        if (effect.targetAttribute != null)
        {
            effect.targetAttribute.RemoveModification(effect);
            var runtimeAttribute = GetRuntimeAttribute(effect.targetAttribute);
            runtimeAttribute?.RecalculateCurrentValue();
        }
        _activeEffects.Remove(effect);
        Debug.Log($"Removed effect: {effect.effectName} for {this} with effect = {effect}");
        
        // Mark as disposed
        effect._disposed = true;
        Debug.Log($"effect set to dispose: {effect.effectName} {effect._disposed}");
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
/*
[CustomPropertyDrawer(typeof(GameplayEffect))]
public class GameplayEffectPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        var effect = property.objectReferenceValue as GameplayEffect;
        if (effect != null)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, $"Effect: {effect.effectName}");
            
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, $"Target: {effect.targetAttribute?.Name}");
            
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, $"Type: {effect.durationType}");
            
            position.y += EditorGUIUtility.singleLineHeight;
            if (effect.valueStrategy != null)
            {
                EditorGUI.LabelField(position, $"Strategy: {effect.valueStrategy.GetType().Name}");
            }
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 4;
    }
}
*/