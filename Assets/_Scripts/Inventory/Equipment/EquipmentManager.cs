using System.Collections.Generic;
using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    private Dictionary<EEquipSlot, IEquipable> equipment;
    public static event Action<IEquipable> OnEquip;
    public List<Item> itemsEquipped;

    private void Awake()
    {
        equipment = new();
        equipment.Add(EEquipSlot.Weapon, null);
    }

    private void OnEnable()
    {
        OnEquip += Equip;
    }

    private void OnDisable()
    {
        OnEquip -= Equip;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            DisplayAllItems();
        }
    }

    public void EquipItem(IEquipable inItem)
    {
        OnEquip?.Invoke(inItem);
    }

    public void Equip(IEquipable itemToEquip)
    {
        EEquipSlot slot = itemToEquip.GetSlot();

        if (equipment[slot] == itemToEquip)
            return;

        if (equipment.ContainsKey(slot) && equipment[slot] != null)
        {
            Unequip(equipment[slot]);
        }

        equipment[slot] = itemToEquip;
        itemsEquipped.Add(itemToEquip as Weapon);
        OnEquip?.Invoke(itemToEquip);
    }

    public void Unequip(IEquipable itemToUnequip)
    {
        EEquipSlot slot = itemToUnequip.GetSlot();

        if (equipment.ContainsKey(slot) && equipment[slot] == itemToUnequip)
        {
            equipment.Remove(slot);
            OnEquip?.Invoke(itemToUnequip);
        }
    }

    public IEquipable GetEquippedItem(EEquipSlot inSlot)
    {
        return equipment.ContainsKey(inSlot) ? equipment[inSlot] : null;
    }

    void DisplayAllItems()
    {
        string heldEquipment = "Held Equipment: ";
        foreach (var pair in equipment)
        {
            //heldEquipment += $"{pair.Key} - {pair.Value}/{pair.Value.Damage}, ";
        }
        Debug.Log(heldEquipment);
    }
}