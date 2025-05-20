using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayAttributeComponent : MonoBehaviour, IDataPersistance
{
    [SerializeField] public List<GameplayAttribute> attributesToAdd = new();
    private Dictionary<string, GameplayAttribute> _attributes = new();
    
    public delegate float AttributeModifierDelegate(GameplayAttribute attribute, float value);
    private event AttributeModifierDelegate OnPreAttributeModified; //Action<GameplayAttribute>
    private event AttributeModifierDelegate OnPostAttributeModified;

    public GameplayEffect _derivedAttributeEffect;
    private GameplayEffect _activeDerivedEffect;
    public GameplayEffect _fullHealEffect;
    
    [SerializeField] public List<GameplayEffect> _activeEffects = new();

    private void Awake()
    {
        OnPreAttributeModified += PreAttributeModificaton;
        OnPostAttributeModified += PostAttributeModificaton;

        InitializeRuntimeAttributes();
        
        if (_derivedAttributeEffect != null)
        {
            Debug.Log("DerivedEffect initializing");
            StartCoroutine(InitializeDerivedAttributes());
            //UpdateDerivedAttributes();
        }
        //ApplyEffect(_derivedAttributeEffect);
    }

    private void OnEnable()
    {
        //StartCoroutine("FullHeal", 5);
    }

    private IEnumerator InitializeDerivedAttributes()
    {
        yield return null;
        UpdateDerivedAttributes();
        //ApplyEffect(_derivedAttributeEffect.Clone(), true);
    }

    private IEnumerator FullHeal()
    {
        ApplyEffect(_fullHealEffect, false);
        yield return null;
    }
    
    public void UpdateDerivedAttributes()
    {/*
        if (_activeDerivedEffect != null)
        {
            // We need to manually remove the applications since the effect might not be in _activeEffects
            foreach (var application in _activeDerivedEffect.applications)
            {
                if (application.targetAttribute != null)
                {
                    application.targetAttribute.RemoveModification(application);
                }
            }
            
            // Also remove from the active effects list if it's there
            _activeEffects.Remove(_activeDerivedEffect);
            _activeDerivedEffect._disposed = true;
            
            Debug.Log($"Removed previous derived attributes effect");
        }
        
        // Step 2: Create a new effect
        if (_derivedAttributeEffect != null)
        {
            _activeDerivedEffect = _derivedAttributeEffect.Clone();
            _activeDerivedEffect.effectName = "DerivedAttributes_" + System.Guid.NewGuid().ToString();
            
            // Apply the new effect
            ApplyDerivedEffect(_activeDerivedEffect);
            
            Debug.Log($"Applied new derived attributes effect: {_activeDerivedEffect.effectName}");
        }*/
        if (_activeDerivedEffect != null)
        {
            foreach (var application in _activeDerivedEffect.applications)
            {
                if (application.targetAttribute != null)
                {
                    application.targetAttribute.RemoveModification(application);
                }
            }
            _activeEffects.Remove(_activeDerivedEffect);
            _activeDerivedEffect._disposed = true;
            
            Debug.Log($"Removed previous derived attributes effect");
        }
        
        // Create and apply a new derived effect
        if (_derivedAttributeEffect != null)
        {
            // Create a new instance
            _activeDerivedEffect = _derivedAttributeEffect.Clone();
            _activeDerivedEffect.effectName = "DerivedAttributes_" + System.Guid.NewGuid().ToString();
            
            // Update all source attributes for AttributeBasedValueStrategy instances
            foreach (var application in _activeDerivedEffect.applications)
            {
                // Make sure target attribute is a runtime attribute
                if (application.targetAttribute != null)
                {
                    application.targetAttribute = GetRuntimeAttribute(application.targetAttribute);
                }
                
                // Fix the source attribute references in AttributeBasedValueStrategy
                if (application.valueStrategy is AttributeBasedValueStrategy attributeStrategy)
                {
                    if (attributeStrategy.sourceAttribute != null)
                    {
                        // Replace with runtime attribute
                        attributeStrategy.sourceAttribute = GetRuntimeAttribute(attributeStrategy.sourceAttribute);
                        Debug.Log($"Updated source attribute reference: {attributeStrategy.sourceAttribute.Name}");
                    }
                }
            }
            // Apply the new effect with fixed attribute references
            ApplyDerivedEffect(_activeDerivedEffect);
            Debug.Log($"Applied new derived attributes effect: {_activeDerivedEffect.effectName}");
        }
    }
    
    private void ApplyDerivedEffect(GameplayEffect effect)
    {
        if (effect == null) return;
        
        // Add to active effects list
        _activeEffects.Add(effect);
        
        // Initialize effect
        effect.Initialize(effect.effectName, effect.durationType, effect.duration, 
            effect.period, effect.modifierType, effect.valueStrategy, 
            effect.stackingType, this, effect.Source);
        
        // Apply each modifier to its target attribute
        foreach (var modifier in effect.applications)
        {
            // Log the value being computed from the strategy
            if (modifier.valueStrategy is AttributeBasedValueStrategy attributeStrategy)
            {
                float sourceValue = attributeStrategy.sourceAttribute.CurrentValue;
                float coefficient = attributeStrategy._coefficient;
                float computedValue = sourceValue * coefficient;
                
                Debug.Log($"Derived attribute calculation: {modifier.targetAttribute.Name} = " +
                          $"{attributeStrategy.sourceAttribute.Name} ({sourceValue}) × {coefficient} = {computedValue}");
            }
            modifier.ApplyModification(this, false);
        }
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
            attributeInstance.Initialize(attribute.Name, attribute.BaseValue);
            _attributes[attributeInstance.Name] = attributeInstance;
        }
    }
    
    public GameplayAttribute GetRuntimeAttribute(GameplayAttribute attributeAsset)
    {
        if (attributeAsset == null)
        {
            Debug.LogWarning("Attempting to get runtime attribute for null attribute");
            return null;
        }
        return _attributes.TryGetValue(attributeAsset.Name, out var runtimeAttr) 
            ? runtimeAttr 
            : attributeAsset;
    }

    public GameplayAttribute GetAttribute(string attributeName)
    {
        return _attributes.TryGetValue(attributeName, out var attribute) ? attribute : null;
    }

    protected virtual float PreAttributeModificaton(GameplayAttribute attribute, float value)
    {
        Debug.Log("PreModification:");/*
        if (attribute.Name == "Health")
        {
            value = Mathf.Max(value, 0);
        }*/
        return value;
    }
    
    protected virtual float PostAttributeModificaton(GameplayAttribute attribute, float value)
    {
        Debug.Log("PostModification");/*
        if (attribute.Name == "Health")
        {
            var maxHealth = GetAttribute("MaxHealth");
            if (maxHealth != null)
            {
                value = Mathf.Clamp(value, 0, maxHealth.CurrentValue());
            }
        }*/
        return value;
    }

    public void ModifyAttribute(GameplayAttribute attribute, float value, EModifierOperationType modType, bool modifyBase, GameplayEffectApplication modification = null)
    {
        if (_attributes.TryGetValue(attribute.Name, out var runtimeAttribute))
        {/*
            OnPreAttributeModified?.Invoke(attribute);
            modifyBase ? runtimeAttribute.SetValue(value, modifyBase) : runtimeAttribute.AddModification(
                new GameplayEffectApplication(
                    runtimeAttribute, 
                    modType, 
                    new ConstantValueStrategy {value = value };
                ));
            OnPostAttributeModified?.Invoke(attribute);*/
            
            if (modifyBase)
            {
                float modifiedValue = OnPreAttributeModified != null ? 
                    OnPreAttributeModified(attribute, value) 
                    : value;
                
                float finalValue = runtimeAttribute.CalculateModifiedValue(runtimeAttribute.CurrentValue, modifiedValue, modType);
                
                finalValue = OnPostAttributeModified != null ? 
                    OnPostAttributeModified(attribute, finalValue) 
                    : finalValue;
                
                runtimeAttribute.SetValue(finalValue, modifyBase);
            }
            else
            {
                runtimeAttribute.AddModification(modification);
            }
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
        var instancedEffect = isPreCreated ? effect : effect.Clone();
        
        instancedEffect.Initialize(instancedEffect.effectName, effect.durationType, effect.duration, effect.period, effect.modifierType, effect.valueStrategy, effect.stackingType, this, effect.Source);
        Debug.Log($"[{Time.frameCount}] Adding to _activeEffects: {instancedEffect.effectName}");
        _activeEffects.Add(instancedEffect);
        
        foreach (var modifier in instancedEffect.applications)
        {
            if (_attributes.TryGetValue(modifier.targetAttribute.Name, out var runtimeAttribute))
            {
                Debug.Log($"Found runtime attribute: {modifier.targetAttribute.Name}");
                modifier.targetAttribute = runtimeAttribute;
                modifier.ApplyModification(this, instancedEffect.durationType == EEffectDurationType.Instant);
            }
        }

        if (instancedEffect.durationType == EEffectDurationType.Instant)
        {
            Debug.Log($"Effect is instant, removing from _activeEffects: {instancedEffect.effectName}");
            RemoveEffect(instancedEffect);
        }
        else { StartCoroutine(HandleDurationEffect(instancedEffect)); }
    }

    private IEnumerator HandleDurationEffect(GameplayEffect effect)
    {
        Debug.Log($"Starting duration effect: {effect.effectName}");
        float elapsedTime = 0;
        float periodTime = 0;

        while (!effect._disposed && (effect.durationType == EEffectDurationType.Infinite || elapsedTime < effect.duration))
        {
            elapsedTime += Time.deltaTime;

            if (!effect._disposed && effect.period > 0)
            {
                periodTime += Time.deltaTime;
                if (periodTime >= effect.period)
                {
                    foreach (var modification in effect.applications)
                    {
                        modification.ApplyModification(this, effect.durationType == EEffectDurationType.Instant);
                    }
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

        if (!_activeEffects.Contains(effect))
        {
            Debug.Log($"Effect is already in _activeEffects, removing from _activeEffects: {effect.effectName}");
            return;
        }
        //Remove effect
        foreach (var modification in effect.applications)
        {
            if (modification.targetAttribute != null)
            {
                Debug.Log($"Removing modification from {modification.targetAttribute.Name}");
                modification.targetAttribute.RemoveModification(modification);
            }
        }
        _activeEffects.Remove(effect);
        Debug.Log($"Removed effect: {effect.effectName} for {this} with effect = {effect}");
        
        effect._disposed = true;
        Debug.Log($"effect set to dispose: {effect.effectName} {effect._disposed}");
    }

    public void SaveData(SaveGameData data)
    {
        if (transform.root.name != "Player") return;
        
        List<SerializableAttributeData> savedAttributes = new List<SerializableAttributeData>();
        
        HashSet<string> attributeNames = new HashSet<string>
        {
            "Vitality", "Strength", "Agility", "Intelligence", "Perception", "Luck", "Health", "Stamina"
        };

        foreach (var attribute in _attributes)
        {
            if (attributeNames.Contains(attribute.Key))
            {
                savedAttributes.Add(new SerializableAttributeData
                {
                    attributeName = attribute.Key,
                    baseValue = attribute.Value.BaseValue
                });
            }
        }

        HashSet<string> effectsToExclude = new HashSet<string>();

        if (TryGetComponent<EquipmentManager>(out var equipmentManager) && equipmentManager.GetEquippedEffects().Count > 0)
        {
            foreach (var equippedEffect in equipmentManager.GetEquippedEffects())
            {
                if (equippedEffect != null)
                {
                    effectsToExclude.Add(equippedEffect.effectName);
                }
            }
        }

        if (_derivedAttributeEffect != null)
        {
            effectsToExclude.Add(_derivedAttributeEffect.effectName);
        }
        
        List<SerializableEffectData> savedEffects = new();
        
        foreach (var effect in _activeEffects)
        {
            if (effect._disposed || effect.durationType == EEffectDurationType.Instant) continue;
            
            if (effectsToExclude.Contains(effect.effectName)) continue;

            SerializableEffectData savedEffectData = new SerializableEffectData
            {
                effectName = effect.effectName,
                durationType = effect.durationType,
                duration = effect.duration,
                period = effect.period,
                modifierType = effect.modifierType,
                source = effect.Source != null ? new SerializableObjectData(effect.Source) : null//new SerializableObjectData(effect.Source)//effect.Source != null ? effect.Source : null
            };

            foreach (var modifier in effect.applications)
            {
                SerializableEffectApplicationData savedModifierData = new SerializableEffectApplicationData
                {
                    targetAttributeName = modifier.targetAttribute.Name,
                    modifierType = modifier.modifierOperation
                };

                if (modifier.valueStrategy is ConstantValueStrategy constantStrategy)
                {
                    savedModifierData.valueStrategyType = "ConstantValueStrategy";
                    savedModifierData.constantValue = constantStrategy.value;
                }
                else if (modifier.valueStrategy is AttributeBasedValueStrategy attributeStrategy)
                {
                    savedModifierData.valueStrategyType = "AttributeBasedValueStrategy";
                    savedModifierData.sourceAttributeName = attributeStrategy.sourceAttribute.Name;
                    savedModifierData.coefficient = attributeStrategy._coefficient;
                }
                savedEffectData.applications.Add(savedModifierData);
            }
            savedEffects.Add(savedEffectData);
        }
        
        Debug.Log($"Saved attributes: {savedAttributes.Count}");
        Debug.Log($"Saved effects: {savedEffects.Count}");

        SerializableSaveAttribue listOfSavedAttributes = new SerializableSaveAttribue();
        listOfSavedAttributes.attributes = savedAttributes;
        data.savedAttributes[gameObject.name] = listOfSavedAttributes;
        //data.savedAttributes[gameObject.name] = savedAttributes;
        //data.savedEffects[gameObject.name] = savedEffects;
        SerializableSaveEffect listOfSavedEffects = new SerializableSaveEffect();
        listOfSavedEffects.effects = savedEffects;
        data.savedEffects[gameObject.name] = listOfSavedEffects;
    }
    
    public void LoadData(SaveGameData data)
    {
        if (transform.root.name != "Player") return;
        
        if (data.savedAttributes.TryGetValue(gameObject.name, out var savedAttributes))
        {
            foreach (var attributeToFind in savedAttributes.attributes)
            {
                if (_attributes.TryGetValue(attributeToFind.attributeName, out var attribute))
                {
                    attribute.SetValue(attributeToFind.baseValue, true);
                }
            }
        }

        if (data.savedEffects.TryGetValue(gameObject.name, out var savedEffects))
        {
            foreach (var activeEffect in new List<GameplayEffect>(_activeEffects))
            {
                RemoveEffect(activeEffect);
            }

            foreach (var savedEffect in savedEffects.effects)
            {
                GameplayEffect newEffect = ScriptableObject.CreateInstance<GameplayEffect>();
                newEffect.effectName = savedEffect.effectName;
                newEffect.durationType = savedEffect.durationType;
                newEffect.duration = savedEffect.duration;
                newEffect.period = savedEffect.period;
                newEffect.modifierType = savedEffect.modifierType;
                /*
                if (!string.IsNullOrEmpty(savedEffect.source))
                {
                    GameObject sourceObj = GameObject.Find(savedEffect.source);
                    if (sourceObj != null)
                    {
                        newEffect.Source = sourceObj;
                    }
                }*
                if (savedEffect.source != null)
                {
                    object sourceObj = savedEffect.source.FindReferencedObject();
                    newEffect.Source = sourceObj;
                }*/
                if (savedEffect.source != null && !string.IsNullOrEmpty(savedEffect.source.objectType))
                {
                    newEffect.Source = ReconstructSourceObject(savedEffect.source);
                }

                foreach (var modifierData in savedEffect.applications)
                {
                    GameplayAttribute targetAttribute = GetAttribute(modifierData.targetAttributeName);
                    if (targetAttribute == null) continue;

                    IAttributeMagnitudeStrategy valueStrategy = null;

                    if (modifierData.valueStrategyType == "ConstantValueStrategy")
                    {
                        valueStrategy = new ConstantValueStrategy { value = modifierData.constantValue };
                    }
                    else if (modifierData.valueStrategyType == "AttributeBasedValueStrategy")
                    {
                        GameplayAttribute sourceAttribute = GetAttribute(modifierData.sourceAttributeName);
                        if (sourceAttribute != null)
                        {
                            valueStrategy = new AttributeBasedValueStrategy
                            {
                                sourceAttribute = sourceAttribute,
                                _coefficient = modifierData.coefficient
                            };
                        }
                    }

                    if (valueStrategy != null)
                    {
                        GameplayEffectApplication modifier = new GameplayEffectApplication(
                            targetAttribute,
                            modifierData.modifierType,
                            valueStrategy
                        );
                        
                        newEffect.applications.Add(modifier);
                    }
                }

                if (newEffect.applications.Count > 0)
                {
                    ApplyEffect(newEffect, true);
                }
            }
        }
    }

    private object ReconstructSourceObject(SerializableObjectData source)
    {
        if (source.objectType.Contains("PlayerController"))
        {
            return GameManager.instance.player.GetComponent<PlayerController>();
        }
        else if (source.objectType.Contains("GameManager"))
        {
            return GameManager.instance;
        }

        try
        {
            System.Type type = System.Type.GetType(source.objectType);
            if (type != null)
            {
                object instance = System.Activator.CreateInstance(type);

                foreach (var prop in source.properties)
                {
                    var property = type.GetProperty(prop.Key);
                    if (property != null)
                    {
                        try
                        {
                            var convertedValue = System.Convert.ChangeType(prop.Value, property.PropertyType);
                            property.SetValue(instance, convertedValue);
                        }
                        catch { }
                    }
                }
                return instance;
            }
        }
        catch
        {
            Debug.LogWarning($"Failed to reconstruct source object of tpye {source.objectType}");
        }
        return null;
    }
}