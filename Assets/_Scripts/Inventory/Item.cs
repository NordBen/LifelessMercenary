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
    [SerializeField] protected List<EquipmentEffectData> equipmentEffects = new();

    public virtual void Use()
    {
        Debug.Log($"Used {this.name}");
    }

    public virtual void Interact()
    {
        Debug.Log($"Interacted with {this.name}");
        GameManager.instance.player.GetInventoryManager().AddItem(this);
    }
    
    public virtual List<GameplayEffect> CreateItemEffects()
    {
        if (equipmentEffects == null || equipmentEffects.Count == 0)
            return new List<GameplayEffect>();
        
        var effects = new List<GameplayEffect>();
        foreach (var effectData in equipmentEffects)
        {
            var effectStrategy = new ConstantValueStrategy() { value = effectData.value};
            effects.Add(GameplayEffectFactory.CreateEffect(
                effectData.effectName,
                EEffectDurationType.Infinite,
                0,
                0,
                EModifierOperationType.Add,
                effectData.targetAttribute,
                effectStrategy
            ));
        }
        return effects;
    }


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

[Serializable]
public class EquipmentEffectData
{
    public string effectName;
    public GameplayAttribute targetAttribute;
    public float value;
}
