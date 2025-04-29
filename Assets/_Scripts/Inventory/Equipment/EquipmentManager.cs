using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using StarterAssets;

public class EquipmentManager : MonoBehaviour
{
    private Dictionary<EEquipSlot, IEquipable> equipment;
    private Dictionary<IEquipable, List<GameplayEffect>> equippedEffects;
    [SerializeField] private List<GameplayEffect> equippedEffectsList;
    public event Action<IEquipable> OnEquip;
    public Item[] itemsEquipped;

    [SerializeField] private GameObject equipmentScreen;

    [SerializeField] Transform head, torso, leggs, boots, amulet, weapon;
    private Dictionary<EEquipSlot, Transform> slotPlacement;

    [SerializeField] GameObject weaponPrefab, armorPrefab;

    [SerializeField] private Transform _quickbar;

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
        itemsEquipped = new Item[equipment.Count];
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

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleEquipmentMenu();
            ToggleEquipbar();
        }
    }

    private void ToggleEquipbar()
    {
        foreach (Transform barSlot in _quickbar)
        {
            var child = barSlot.GetChild(0);
            GameObject visuals = child.gameObject;
            visuals.SetActive(!visuals.activeSelf);
        }
    }

    private void ToggleEquipmentMenu()
    {
        //GameManager.instance.ToggleQuickbar();
        equipmentScreen.SetActive(!equipmentScreen.activeSelf);
        Debug.Log(equipmentScreen.activeSelf);
        Cursor.lockState = equipmentScreen.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = equipmentScreen.activeSelf;
        GetComponent<StarterAssetsInputs>().cursorLocked = !equipmentScreen.activeSelf;
        GetComponent<StarterAssetsInputs>().cursorInputForLook = !equipmentScreen.activeSelf;
    }

    public void TryEquip(IEquipable inItem)
    {
        OnEquip?.Invoke(inItem);
    }

    public static T ConvertTo<T>(object value)
    {
        return (T)Convert.ChangeType(value, typeof(T));
    }

    private void Equip(IEquipable itemToEquip)
    {
        EEquipSlot slot = itemToEquip.GetSlot();

        if (equipment.ContainsKey(slot) && equipment[slot] != null)
        {
            if (equipment[slot] == itemToEquip)
            {
                Unequip(equipment[slot]);
                return;
            }

            Unequip(equipment[slot]);
        }

        GameObject physicalEquipment = null;
        equipment[slot] = itemToEquip;
        switch (itemToEquip.GetSlot())
        {
            case EEquipSlot.Weapon:
                itemsEquipped[SlotIndex(slot)] = itemToEquip as Weapon;
                physicalEquipment = Instantiate(weaponPrefab);
                if (physicalEquipment != null)
                {
                    var weaponObject = physicalEquipment.GetComponent<WeaponObject>();
                    if (weaponObject != null)
                    {
                        weaponObject.SetWeaponData(itemToEquip as Weapon);
                        var combatManager = GameManager.instance?.player?.GetCombatManager();
                        if (combatManager != null)
                        {
                            combatManager.weapon = weaponObject;
                        }
                        else
                        {
                            Debug.LogError("Combat Manager not found!");
                        }
                    }
                    else
                    {
                        Debug.LogError("WeaponObject component not found on instantiated weapon!");
                    }
                } /*
            physicalEquipment.GetComponent<WeaponObject>().SetWeaponData(itemToEquip as Weapon);
            GameManager.instance.player.GetCombatManager().weapon = physicalEquipment.GetComponent<WeaponObject>();*/
                break;
            case EEquipSlot.Head:
            case EEquipSlot.Torso:
            case EEquipSlot.Leggs:
            case EEquipSlot.Boots:
                itemsEquipped[SlotIndex(slot)] = itemToEquip as Armor;
                physicalEquipment = Instantiate(armorPrefab);
                break;
            default:
                break;
        }

        if (physicalEquipment != null)
            AttachEquipment(physicalEquipment.transform, itemToEquip.GetSlot());
        
        OnEquipItem(itemToEquip);
    }

    private void AttachEquipment(Transform equipmentToAttach, EEquipSlot slotToPlace)
    {
        equipmentToAttach.parent = slotPlacement[slotToPlace];
        equipmentToAttach.localRotation = Quaternion.Euler(Vector3.zero);

        if (slotToPlace != EEquipSlot.Head)
        {
            equipmentToAttach.localPosition = Vector3.zero;// new Vector3(0f, 0f, 0f);
        }
        else
        {
            equipmentToAttach.localPosition = new Vector3(0f, -1.635f, 0.041f);
        }
    }

    private int SlotIndex(EEquipSlot slot)
    {
        int index = slot switch
        { EEquipSlot.Weapon => 0,
          EEquipSlot.Head => 1,
          EEquipSlot.Torso => 2,
          EEquipSlot.Leggs => 3,
          EEquipSlot.Boots => 4,
          EEquipSlot.Amulet => 5,
          _ => -1 };
        return index;
    }

    private void Unequip(IEquipable itemToUnequip)
    {
        EEquipSlot slot = itemToUnequip.GetSlot();

        if (equipment.ContainsKey(slot) && equipment[slot] == itemToUnequip)
        {
            if (equipment[slot] != null)
            {
                Destroy(slotPlacement[slot].GetChild(0).gameObject);
            }
            equipment[slot] = null;
            itemsEquipped[SlotIndex(slot)] = null;
        }
        
        OnUnequipItem(itemToUnequip);
    }

    public IEquipable GetEquippedItem(EEquipSlot inSlot)
    {
        return equipment.ContainsKey(inSlot) ? equipment[inSlot] : null;
    }

    public void OnEquipItem(IEquipable item)
    {/*
        if (itemsEquipped[SlotIndex(item.GetSlot())] != null)
        {
            OnUnequipItem(item);
        }*/
        
        var effects = itemsEquipped[SlotIndex(item.GetSlot())].CreateItemEffects();
        //equippedEffects[item] = effects;
        foreach (var effect in effects)
        {
            effect.Source = item;
            equippedEffectsList.Add(effect);
            GetComponent<GameplayAttributeComponent>().ApplyEffect(effect, true);
        }
    }

    public void OnUnequipItem(IEquipable item)
    {/*
        if (equippedEffects.TryGetValue(item, out var effects))
        {
        }*/
        /*
        var item = itemsEquipped[slot];
        if (item == null) return;*/

        // Remove effects associated with this item
        var effectsToRemove = equippedEffectsList.Where(effect => effect.Source == item).ToList();
        foreach (var effect in effectsToRemove)
        {
            equippedEffectsList.Remove(effect);
            GetComponent<GameplayAttributeComponent>().RemoveEffect(effect);
        }
        //equippedItems[slot] = null;
    }
}