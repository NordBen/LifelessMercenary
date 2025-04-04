using UnityEngine;

[System.Serializable]
public enum EItemType
{
    Basic,
    Weapon,
    Armor,
    Quest
}

[System.Serializable]
[CreateAssetMenu(fileName = "Item", menuName = "Item/Item")]
public abstract class Item : ScriptableObject, IInteractable
{
    public string itemName;
    public Sprite icon;
    public Mesh mesh;
    public EItemType type;
    public float sellValue;

    public virtual void Use()
    {
        Debug.Log($"Used {this.name}");
    }

    public virtual void Interact()
    {
        Debug.Log($"Interacted with {this.name}");
    }
}