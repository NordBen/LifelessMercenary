using System.Collections.Generic;
using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    private Dictionary<EEquipSlot, IEquipable> equipment;
    public event Action<IEquipable> OnEquip;
    public List<Item> itemsEquipped;

    [SerializeField] Transform head, torso, leggs, boots, amulet, weapon;
    private Dictionary<EEquipSlot, Transform> slotPlacement;

    [SerializeField] GameObject weaponPrefab, armorPrefab;

    private void Awake()
    {
        equipment = new();
        equipment.Add(EEquipSlot.Head, null);
        equipment.Add(EEquipSlot.Torso, null);
        equipment.Add(EEquipSlot.Leggs, null);
        equipment.Add(EEquipSlot.Boots, null);
        equipment.Add(EEquipSlot.Amulet, null);
        equipment.Add(EEquipSlot.Weapon, null);
        slotPlacement = new();
        slotPlacement[EEquipSlot.Head] = head;
        slotPlacement[EEquipSlot.Torso] = torso;
        slotPlacement[EEquipSlot.Leggs] = leggs;
        slotPlacement[EEquipSlot.Boots] = boots;
        slotPlacement[EEquipSlot.Amulet] = amulet;
        slotPlacement[EEquipSlot.Weapon] = weapon;
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

    public static T ConvertTo<T>(object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
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
        switch (itemToEquip.GetSlot())
        {
            case EEquipSlot.Weapon:
                itemsEquipped.Add(itemToEquip as Weapon);
                break;
            case EEquipSlot.Head:
            case EEquipSlot.Torso:
            case EEquipSlot.Leggs:
            case EEquipSlot.Boots:
                itemsEquipped.Add(itemToEquip as Armor);
                break;
            default:
                break;
        }

        GameObject newEquipment = Instantiate(itemToEquip.GetSlot() == EEquipSlot.Weapon ? weaponPrefab : armorPrefab);
        if (itemToEquip.GetSlot() == EEquipSlot.Weapon) 
            newEquipment.GetComponent<WeaponObject>().SetWeaponData(itemsEquipped[0] as Weapon);
        GameManager.instance.player.GetCombatManager().weapon = newEquipment;

        OnEquip?.Invoke(itemToEquip);

        AttachEquipment(newEquipment.transform, itemToEquip.GetSlot());
    }

    private void AttachEquipment(Transform equipmentToAttach, EEquipSlot slotToPlace)
    {
        equipmentToAttach.parent = slotPlacement[slotToPlace];
        equipmentToAttach.localPosition = Vector3.zero;// new Vector3(0f, 0f, 0f);
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