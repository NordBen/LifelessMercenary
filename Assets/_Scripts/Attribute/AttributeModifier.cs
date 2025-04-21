using System;
using System.Collections;
using UnityEngine;

[System.Serializable]
public class AttributeModifier
{
    public float value;
    public string attributeToModify;
    public EModifierType type;
    public EModifierDuration applicationType;
    public float duration;
    public object source;

    public AttributeModifier(float newValue, string inAttributeToModify, EModifierType newType, EModifierDuration newApplicationType, float newDuration, object inSource = null)
    {
        this.value = newValue;
        this.attributeToModify = inAttributeToModify;
        this.type = newType;
        this.applicationType = newApplicationType;
        this.duration = newDuration;
        this.source = inSource;
    }

    public IEnumerable Duration()
    {
        yield return new WaitForSeconds(this.duration);
    }

    public IEnumerator ApplyBuff(Attribute attribute, Action<AttributeModifier> onBuffEnd)
    {
        if (applicationType == EModifierDuration.Instant)
        {
            attribute.AddModifier(this);
            float elapsedTime = 0;
            float tickInterval = 1.0f;

            while (elapsedTime < duration)
            {
                yield return new WaitForSeconds(tickInterval);
                elapsedTime += tickInterval;
            }

            attribute.RemoveModifier(this);
            onBuffEnd?.Invoke(this);
        }
    }
}