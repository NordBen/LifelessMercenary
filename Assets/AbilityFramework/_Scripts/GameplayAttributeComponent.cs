using System.Collections;
using System.Collections.Generic;
using LM.Inventory;
using UnityEngine;

namespace LM.AbilitySystem
{
    public class GameplayAttributeComponent : MonoBehaviour, IDataPersistance
    {
        [SerializeField] public List<GameplayAttribute> attributesToAdd = new();
        private Dictionary<string, GameplayAttribute> _attributes = new();

        public delegate float AttributeModifierDelegate(GameplayAttribute attribute, float value);
        private event AttributeModifierDelegate OnPreAttributeModified;
        private event AttributeModifierDelegate OnPostAttributeModified;

        public GameplayEffect _derivedAttributeEffect;
        private GameplayEffect _activeDerivedEffect;
        public GameplayEffect _fullHealEffect;

        [SerializeField] public List<GameplayEffect> _activeEffects = new();

        private void Awake()
        {
            InitializeRuntimeAttributes();

            if (_derivedAttributeEffect != null)
            {
               Invoke("InitStats", 1f);
            }
        }

        private void InitStats()
        {
            Debug.Log("DerivedEffect initializing");
            StartCoroutine(InitializeDerivedAttributes());
        }

        private void OnEnable()
        {
            OnPreAttributeModified += PreAttributeModificaton;
            OnPostAttributeModified += PostAttributeModificaton;
        }
        
        private void OnDisable()
        {
            OnPreAttributeModified -= PreAttributeModificaton;
            OnPostAttributeModified -= PostAttributeModificaton;
        }

        private IEnumerator InitializeDerivedAttributes()
        {
            yield return null;
            ApplyEffect(_derivedAttributeEffect, false);
        }

        public void UpdateDerivedAttributes()
        {
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
                Debug.Log($"Removed previous derived attributes effect");
            }
            
            if (_derivedAttributeEffect != null)
            {
                _activeDerivedEffect = _derivedAttributeEffect.Clone();
                _activeDerivedEffect.effectName = "DerivedAttributes_" + System.Guid.NewGuid().ToString();
                
                foreach (var application in _activeDerivedEffect.applications)
                {
                    if (application.targetAttribute != null)
                    {
                        application.targetAttribute = GetRuntimeAttribute(application.targetAttribute);
                    }
                    
                    if (application.valueStrategy is AttributeBasedValueStrategy attributeStrategy)
                    {
                        if (attributeStrategy.sourceAttribute != null)
                        {
                            attributeStrategy.sourceAttribute = GetRuntimeAttribute(attributeStrategy.sourceAttribute);
                            Debug.Log($"Updated source attribute reference: {attributeStrategy.sourceAttribute.Name}");
                        }
                    }
                }
                ApplyEffect(_activeDerivedEffect);
                Debug.Log($"Applied new derived attributes effect: {_activeDerivedEffect.effectName}");
            }
        }

        private void InitializeRuntimeAttributes()
        {
            Debug.Log("Initializing runtime attributes");
            foreach (var attribute in attributesToAdd)
            {
                var attributeInstance = ScriptableObject.CreateInstance<GameplayAttribute>();
                attributeInstance.Initialize(attribute.Name, attribute.BaseValue);
                _attributes[attributeInstance.Name] = attributeInstance;
            }
        }

        public GameplayAttribute GetRuntimeAttribute(GameplayAttribute attributeAsset)
        {
            if (attributeAsset == null) return null;
            return _attributes.TryGetValue(attributeAsset.Name, out var runtimeAttr) ? runtimeAttr : attributeAsset;
        }

        public GameplayAttribute GetAttribute(string attributeName) => _attributes.TryGetValue(attributeName, out var attribute) ? attribute : null;

        protected virtual float PreAttributeModificaton(GameplayAttribute attribute, float value)
        {
            Debug.Log("PreModification:"); /*
            if (attribute.Name == "Health")
            {
                value = Mathf.Max(value, 0);
            }*/
            return value;
        }

        protected virtual float PostAttributeModificaton(GameplayAttribute attribute, float value)
        {
            Debug.Log("PostModification");
            if (attribute.Name == "Health")
            {
                var maxHealth = GetAttribute("MaxHealth");
                if (maxHealth != null)
                {
                    value = Mathf.Clamp(value, 0, maxHealth.CurrentValue);
                }
            }
            return value;
        }

        public void ModifyAttribute(GameplayAttribute attribute, float value, EModifierOperationType modType, bool modifyBase, GameplayEffectApplication modification = null)
        {
            if (_attributes.TryGetValue(attribute.Name, out var runtimeAttribute))
            {
                if (modifyBase)
                {
                    float modifiedValue = OnPreAttributeModified != null
                        ? OnPreAttributeModified(attribute, value)
                        : value;

                    float finalValue =
                        runtimeAttribute.CalculateModifiedValue(runtimeAttribute.CurrentValue, modifiedValue, modType);

                    finalValue = OnPostAttributeModified != null
                        ? OnPostAttributeModified(attribute, finalValue)
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

            instancedEffect.Initialize(instancedEffect.effectName, effect.durationType, effect.duration, effect.period,
                effect.modifierType, effect.valueStrategy, effect.stackingType, this, effect.Source);
            
            Debug.Log($"[{Time.frameCount}] Adding to _activeEffects: {instancedEffect.effectName}");
            _activeEffects.Add(instancedEffect);
            
            instancedEffect.OnApply += ApplyMods;
            instancedEffect.OnRemove += RemoveEffect;
            StartCoroutine(instancedEffect.Apply());

            if (instancedEffect.durationType == EEffectDurationType.Instant)
            {
                Debug.Log($"Effect is instant: {instancedEffect.effectName}");
            }
        }

        private void ApplyMods(GameplayEffect effect)
        {
            foreach (var mod in effect.applications)
            {
                Debug.Log($"attributes : {_attributes.Count}");
                if (_attributes.TryGetValue(mod.targetAttribute.Name, out var runtimeAttribute))
                {
                    Debug.Log($"Found runtime attribute: {runtimeAttribute}");
                    mod.targetAttribute = runtimeAttribute;
                    ModifyAttribute(runtimeAttribute, mod.ComputeValue(this), 
                        mod.modifierOperation, effect.durationType == EEffectDurationType.Instant, mod);
                }
            }
        }
        
        public void RemoveEffect(GameplayEffect effect)
        {
            if (effect == null) return;
            /*
            if (!_activeEffects.Contains(effect))
            {
                Debug.Log($"Effect is already in _activeEffects, removing from _activeEffects: {effect.effectName}");
                return;
            }*/
            foreach (var modification in effect.applications)
                if (modification.targetAttribute != null)
                    modification.targetAttribute.RemoveModification(modification);
            
            _activeEffects.Remove(effect);
            effect.OnApply -= ApplyMods;
            effect.OnRemove -= RemoveEffect;
            StopCoroutine(effect.Apply());
            Debug.Log($"Removed effect: {effect.effectName} for {this} with effect = {effect}");
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

            if (TryGetComponent<EquipmentManager>(out var equipmentManager) &&
                equipmentManager.GetEquippedEffects().Count > 0)
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
                if (effect.durationType == EEffectDurationType.Instant) continue;

                if (effectsToExclude.Contains(effect.effectName)) continue;

                SerializableEffectData savedEffectData = new SerializableEffectData
                {
                    effectName = effect.effectName,
                    durationType = effect.durationType,
                    duration = effect.duration,
                    period = effect.period,
                    modifierType = effect.modifierType,
                    source = effect.Source != null
                        ? new SerializableObjectData(effect.Source)
                        : null //new SerializableObjectData(effect.Source)//effect.Source != null ? effect.Source : null
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
            SerializableSaveEffect listOfSavedEffects = new SerializableSaveEffect();
            listOfSavedEffects.effects = savedEffects;
            data.savedEffects[gameObject.name] = listOfSavedEffects;
        }

        public void LoadData(SaveGameData data)
        {
            if (transform.root.name != "Player") return;
            /*
            var ReInitEffect = EffectFactory.CreateEffect("ReInitSavedStats", EEffectDurationType.Instant, 0, null, 0, this, this);
            var initModifiers = new List<GameplayEffectApplication>();
            if (data.savedAttributes.TryGetValue(gameObject.name, out var savedAttributes))
            {
                foreach (var attributeToFind in savedAttributes.attributes)
                {
                    if (_attributes.TryGetValue(attributeToFind.attributeName, out var attribute))
                    {
                        var strategy = new ConstantValueStrategy { value = attributeToFind.baseValue };
                        var mod = new GameplayEffectApplication(attribute, EModifierOperationType.Override, strategy);
                        initModifiers.Add(mod);
                        Debug.LogError($"Loading data and applying a new effect for {attributeToFind.attributeName} with value {attributeToFind.baseValue}");
                        //attribute.SetValue(attributeToFind.baseValue, true);
                    }
                }
            }
            ReInitEffect.applications = initModifiers;
            ApplyEffect(ReInitEffect, true);*/

            if (data.savedEffects.TryGetValue(gameObject.name, out var savedEffects))
            {
                foreach (var activeEffect in new List<GameplayEffect>(_activeEffects))
                {
                    RemoveEffect(activeEffect);
                }

                StartCoroutine(InitSavedAttributes(data.savedAttributes));

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

        private IEnumerator InitSavedAttributes(SerializableSaveDictionary<string, SerializableSaveAttribue> savedData)
        {
            yield return null;
            var ReInitEffect = EffectFactory.CreateEffect("ReInitSavedStats", EEffectDurationType.Instant, 0, null, 0, this, this);
            var initModifiers = new List<GameplayEffectApplication>();
            if (savedData.TryGetValue(gameObject.name, out var savedAttributes))
            {
                foreach (var attributeToFind in savedAttributes.attributes)
                {
                    if (_attributes.TryGetValue(attributeToFind.attributeName, out var attribute))
                    {
                        var strategy = new ConstantValueStrategy { value = attributeToFind.baseValue };
                        var mod = new GameplayEffectApplication(attribute, EModifierOperationType.Override, strategy);
                        initModifiers.Add(mod);
                        //Debug.LogError($"Loading data and applying a new effect for {attributeToFind.attributeName} with value {attributeToFind.baseValue}");
                    }
                }
            }
            ReInitEffect.applications = initModifiers;
            ApplyEffect(ReInitEffect, true);
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
                            catch
                            {
                            }
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
}