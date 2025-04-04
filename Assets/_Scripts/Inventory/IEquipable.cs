using UnityEngine;

public interface IEquipable
{
    EEquipSlot GetSlot();
    void Equip();
}