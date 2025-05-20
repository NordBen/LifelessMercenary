using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Armor", menuName = "Item/Armor Piece")]
public class Armor : Item, IEquipable
{
    public float defence;
    public EEquipSlot slot = EEquipSlot.Head;

    public EEquipSlot GetSlot() => slot;
    /*
    public override void Interact()
    {
        Equip();
    }*/

    public void Equip()
    {
        GameManager.instance.player.GetEquipmentManager().TryEquip(this);//.GetComponent<EquipmentManager>().Equip(this);
    }
}