using System.Collections.Generic;
using System;
using UnityEngine;

[System.Serializable]
public class Attribute
{
    protected float baseValue;
    protected float currentValue;
    protected float _modifiedValue;
    List<AttributeModifier> modifiers = new();

    public event Action<float, float> OnValueChanged;

    public Attribute(float defaultValue)
    {
        this.baseValue = defaultValue;
        this.currentValue = this.baseValue;
        this._modifiedValue = 0;
    }

    public float BaseValue() { return baseValue; }
    public float CurrentValue() { return currentValue; }
    public float ModifiedValue() { return _modifiedValue; }

    public void AddModifier(AttributeModifier modifier)
    {
        modifiers.Add(modifier);
        RecalculateAttribute();
    }

    public bool HasModifier(AttributeModifier modifier)
    {
        return modifiers.Contains(modifier);
    }

    public void RemoveModifier(AttributeModifier modifier)
    {
        if (modifiers.Contains(modifier))
        {
            modifiers.Remove(modifier);
        }
        RecalculateAttribute();
    }

    public void SetBaseValue(float newBase)
    {
        this.baseValue = newBase;
        RecalculateAttribute();
    }

    public void ModifyAttribute(float newValue)
    {
        float oldValue = this.currentValue;
        this.currentValue = this.baseValue + newValue;
        OnValueChanged?.Invoke(this.currentValue, oldValue);
        Debug.Log($"New Attribute value for: {this.ToString()}, {this.currentValue}");
    }

    public void AddToModifiedValue(float newValue)
    {
        _modifiedValue = newValue;
        RecalculateAttribute();
    }

    public void SetAttributeDirect(float newValue)
    {
        float oldValue = this.currentValue;
        this.currentValue = newValue;
        OnValueChanged?.Invoke(this.currentValue, oldValue);
    }

    private void RecalculateAttribute()
    {
        Debug.Log($"Recalculating Attribute {modifiers.Count}");
        float additives = 0;
        float percentages = 1;
        float multiplicants = 0;
        foreach (AttributeModifier mod in modifiers)
        {
            switch (mod.type)
            {
                case EModifierType.Add:
                    additives += mod.value;
                    Debug.Log($" newAdditives value = {additives}");
                    break;
                case EModifierType.Percent:
                    percentages += mod.value;
                    Debug.Log($" newPercentages value = {percentages}");
                    break;
                case EModifierType.Multiply:
                    multiplicants += mod.value;
                    Debug.Log($" newMulti value = {multiplicants}");
                    break;
                default:
                    break;
            }
        }
        _modifiedValue = (this.baseValue + additives) * percentages * (multiplicants > 0 ? multiplicants : 1) - this.baseValue;
        Debug.Log($"Final value = {this._modifiedValue}");
        ModifyAttribute(_modifiedValue);
    }
}