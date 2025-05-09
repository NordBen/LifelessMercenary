using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "Item/Item")]
public abstract class Item : ScriptableObject, IInteractable
{
    public string itemName;
    public Sprite icon;
    public Mesh mesh;
    public Material material;
    public EItemType type;
    public EItemGrade grade;
    public bool bIsStackable = false;
    public int quantity;
    public int maxQuantity;
    public float sellValue;
    [SerializeField] protected List<GameplayEffectApplication> equipmentEffect;

    public virtual void Use()
    {
        Debug.Log($"Used {this.name}");
    }

    public virtual void Interact()
    {
        Debug.Log($"Interacted with {this.name}");
        GameManager.instance.player.GetInventoryManager().AddItem(this);
    }

    public virtual GameplayEffect CreateItemEffect()
    {
        if (equipmentEffect == null)
            return null;
        
        var effectApplications = new List<GameplayEffectApplication>();
        foreach (var effectData in equipmentEffect)
        {
            effectApplications.Add(effectData.CloneApplication());
        }
        var effect = GameplayEffectFactory.CreateEffect(
            itemName + "Effect",
            EEffectDurationType.Infinite,
            0f,
            effectApplications,
            0f
        );
        effect.Source = this;
        
        return effect;
    }
    /*
    public virtual List<GameplayEffect> CreateItemEffects()
    {
        /*
        if (equipmentEffects == null || equipmentEffects.Count == 0)
            return new List<GameplayEffect>();*
        var effectApplications = new List<GameplayEffectApplication>();
        var effects = new List<GameplayEffect>();
        foreach (var effectData in equipmentEffect.equipmentApplications)
        {
            var effectApplication = new GameplayEffectApplication
                (
                    effectData.targetAttribute,
                    effectData.operationType,
                    effectData.ValueStrategy
                );
                
            effectApplications.Add(effectApplication);
        }
        var effect = GameplayEffectFactory.CreateEffect(
            equipmentEffect.effectName,
            EEffectDurationType.Infinite,
            0f,
            effectApplications,
            0f
        );
        effect.Source = this;
        effects.Add(effect);
        
        return effects;
    }*/


    public Color GetColorByItemGrade()
    {
        Color gradedColor = this.grade switch
        {
            EItemGrade.Uncommon => Color.gray,
            EItemGrade.Common => Color.green,
            EItemGrade.Great => Color.blue,
            EItemGrade.Epic => Color.red,
            EItemGrade.Unique => Color.magenta,
            EItemGrade.Legendary => Color.yellow,
            _ => Color.white,
        };
        return gradedColor;
    }
}
