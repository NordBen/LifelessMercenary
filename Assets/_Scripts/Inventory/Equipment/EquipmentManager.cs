using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public void Equip(Item itemToEquip)
    {
        Debug.Log($"Equipped {itemToEquip}");
    }
}