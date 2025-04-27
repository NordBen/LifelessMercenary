using System;
using UnityEngine;

public interface IAttributeValueStrategy
{
    float CalculateValue(GameplayAttribute attribute, GameplayEffect effect);
}

[Serializable]
public class ConstantValueStrategy : IAttributeValueStrategy
{
    public float value;
    
    public float CalculateValue(GameplayAttribute attribute, GameplayEffect effect)
    {
        return value;
    }
}

[Serializable]
public class CurveValueStrategy : IAttributeValueStrategy
{
    public AnimationCurve curve;
    public float timeScale = 1f;
    
    public float CalculateValue(GameplayAttribute attribute, GameplayEffect effect)
    {
        return curve.Evaluate(Time.time * timeScale);
    }
}

[Serializable]
public class AttributeBasedValueStrategy : IAttributeValueStrategy
{
    public GameplayAttribute sourceAttribute;
    public float _coefficient;
    
    public float CalculateValue(GameplayAttribute attribute, GameplayEffect effect)
    {
        return sourceAttribute.CurrentValue() * _coefficient;
    }
}