using System;
using System.Collections.Generic;
using UnityEngine;

public interface IAttributeValueStrategy
{
    float CalculateValue(GameplayAttribute attribute, GameplayEffectApplication modification);
}

[Serializable]
public class ConstantValueStrategy : IAttributeValueStrategy
{
    public float value;
    
    public float CalculateValue(GameplayAttribute attribute, GameplayEffectApplication modification)
    {
        return value;
    }
}

[Serializable]
public class CurveValueStrategy : IAttributeValueStrategy
{
    public AnimationCurve curve;
    public float timeScale = 1f;
    
    public float CalculateValue(GameplayAttribute attribute, GameplayEffectApplication modification)
    {
        return curve.Evaluate(Time.time * timeScale);
    }
}

[Serializable]
public class AttributeBasedValueStrategy : IAttributeValueStrategy
{
    public GameplayAttribute sourceAttribute;
    public float _coefficient;
    
    public float CalculateValue(GameplayAttribute attribute, GameplayEffectApplication modification)
    {
        return sourceAttribute.CurrentValue() * _coefficient;
    }
}
/*
[Serializable]
public abstract class GameplayEffectModifierMagnitudeCalculation : IAttributeValueStrategy
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