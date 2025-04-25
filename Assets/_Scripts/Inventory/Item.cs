using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "Item/Item")]
public abstract class Item : ScriptableObject, IInteractable
{
    public string itemName;
    public Sprite icon;
    public Mesh mesh;
    public EItemType type;
    public EItemGrade grade;
    public bool bIsStackable = false;
    public int quantity;
    public int maxQuantity;
    public float sellValue;

    public virtual void Use()
    {
        Debug.Log($"Used {this.name}");
    }

    public virtual void Interact()
    {
        Debug.Log($"Interacted with {this.name}");
        GameManager.instance.player.GetInventoryManager().AddItem(this);
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