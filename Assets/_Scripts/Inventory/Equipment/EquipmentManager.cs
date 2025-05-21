using System.Collections.Generic;
using System;
using System.Linq;
using LM.AbilitySystem;
using UnityEngine;
using StarterAssets;
using LM.Inventory;

public class EquipmentManager : MonoBehaviour//, IDataPersistance
{
    private Dictionary<EEquipSlot, IEquipable> equipment;
    private Dictionary<IEquipable, List<GameplayEffect>> equippedEffects;
    private Dictionary<Item, List<GameplayEffect>> _itemEffectsMap = new Dictionary<Item, List<GameplayEffect>>();
    [SerializeField] private List<GameplayEffect> equippedEffectsList;
    [SerializeField] private List<GameplayEffectApplication> equippedEffectApplications;
    
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
        foreach (EEquipSlot slotType in Enum.GetValues(typeof(EEquipSlot)))
        {
            equipment.Add(slotType, null);
        }/*
        equipment.Add(EEquipSlot.Head, null);
        equipment.Add(EEquipSlot.Torso, null);
        equipment.Add(EEquipSlot.Leggs, null);
        equipment.Add(EEquipSlot.Boots, null);
        equipment.Add(EEquipSlot.Amulet, null);
        equipment.Add(EEquipSlot.Weapon, null);*/
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
    
    public List<GameplayEffect> GetEquippedEffects() => equippedEffectsList;

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
                }
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
        Debug.Log("Unequipping: " + itemToUnequip);
        EEquipSlot slot = itemToUnequip.GetSlot();

        OnUnequipItem(itemToUnequip);
        if (equipment.ContainsKey(slot) && equipment[slot] == itemToUnequip)
        {
            if (equipment[slot] != null)
            {
                Destroy(slotPlacement[slot].GetChild(0).gameObject);
            }
            equipment[slot] = null;
            itemsEquipped[SlotIndex(slot)] = null;
        }
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

        var mappedItem = itemsEquipped[SlotIndex(item.GetSlot())];
        var effects = mappedItem.CreateItemEffect();
        if (effects != null)
        {
            GetComponent<GameplayAttributeComponent>().ApplyEffect(effects, true);
            /*
            foreach (var modification in effects.applications)
            {
                equippedEffectApplications.Add(modification);
            }
            equippedEffectsList.Add(effects);*/
            if (!_itemEffectsMap.ContainsKey(mappedItem))
            {
                _itemEffectsMap[mappedItem] = new List<GameplayEffect>();
            }
            _itemEffectsMap[mappedItem].Add(effects);
        }
        //equippedEffects[item] = effects;
        /*foreach (var effect in effects)
        {
            Debug.Log("Before everyting - found effect with source " + effect.Source + "for item: " + item);
            //effect.Source = item;
            Debug.Log($"Original effect ID: {effect.GetHashCode()}, Source: {effect.Source}, Item: {item}");
            equippedEffectsList.Add(effect);
            Debug.Log("After adding to List - found effect with source " + effect.Source + "for item: " + item);
            GetComponent<GameplayAttributeComponent>().ApplyEffect(effect, true);
            Debug.Log($"After apply effect ID: {effect.GetHashCode()}, Source: {effect.Source}, Item: {item}");
            Debug.Log("After ApplyEffect - Added effect: " + effect.ToString() + "with source: " + effect.Source + "for item: " + item);
        }*/
    }

    public void OnUnequipItem(IEquipable item)
    {/*
        if (equippedEffects.TryGetValue(item, out var effects))
        {
        }*/
        /*
        var item = itemsEquipped[slot];
        if (item == null) return;*

        // Remove effects associated with this item
        var effectsToRemove = equippedEffectsList.Where(effect => effect.Source == item).ToList();
        Debug.Log("Effects to remove: " + effectsToRemove.Count);
        foreach (var effect in effectsToRemove)
        {
            Debug.Log("Removing effect: " + effect.ToString() + "with source: " + effect.Source + "for item: " + item);
            equippedEffectsList.Remove(effect);
            GetComponent<GameplayAttributeComponent>().RemoveEffect(effect);
        }*/
        //equippedItems[slot] = null;

        var mappedItem = itemsEquipped[SlotIndex(item.GetSlot())];
        if (_itemEffectsMap.TryGetValue(mappedItem, out var effects))
        {
            foreach (var effect in effects)
            {
                GetComponent<GameplayAttributeComponent>().RemoveEffect(effect);
            }
            _itemEffectsMap.Remove(mappedItem);
        }
    }
    
    public List<GameplayEffect> GetAllItemEffects()
    {
        List<GameplayEffect> allEffects = new List<GameplayEffect>();
        foreach (var effectsList in _itemEffectsMap.Values)
        {
            allEffects.AddRange(effectsList);
        }
        return allEffects;
    }

    public bool HasActiveEffects(IEquipable item)
    {
        var mappedItem = itemsEquipped[SlotIndex(item.GetSlot())];
        return _itemEffectsMap.ContainsKey(mappedItem) && _itemEffectsMap[mappedItem].Count > 0;
    }
    
    public void UnequipAll()
    {
        List<Item> itemsToUnequip = new List<Item>(itemsEquipped.Length);
        foreach (var item in itemsToUnequip)
        {
            if (item != null)
            {
                Unequip(item as IEquipable);
            }
        }
    }

    public void SaveData(SaveGameData data)
    {
        List<SerializableItem> equipmentData = new List<SerializableItem>();

        foreach (var item in itemsEquipped)
        {
            if (item != null)
            {
                SerializableItem serializedItem = SerializableItem.CreateFrom(item);
                
                equipmentData.Add(serializedItem as SerializableItemEquipable);
            }
        }
        //data.savedEquipment[gameObject.name] = itemsEquipped;
        SerializableItemList equipmentToSave = new SerializableItemList();
        equipmentToSave.items = equipmentData;
        data.savedEquipment[gameObject.name] = equipmentToSave;
    }

    public void LoadData(SaveGameData data)
    {
        if (!data.savedEquipment.ContainsKey(gameObject.name)) return;
        
        var equipmentData = data.savedEquipment; //[gameObject.name]
        
        Dictionary<string, Item> itemsToEquip = new Dictionary<string, Item>();

        if (TryGetComponent<InventoryManager>(out var inventory))
        {
            if (inventory == null)
            {
                Debug.LogError("EquipmentManager could not find InventoryManager for loading equipment");
                return;
            }
            
            foreach (var item in inventory.GetAllItems())
            {
                itemsToEquip[item.itemName] = item;
            }

            foreach (var listOfEquipment in data.savedEquipment.Values)
            {
                foreach (var itemToFind in listOfEquipment.items)
                {
                    Item itemToEquip = inventory.FindItemById(itemToFind.itemId);
                    if (itemToEquip != null)
                    {
                        TryEquip(itemToEquip as IEquipable);
                    }
                }
            }
            /*
            foreach (var slotData in equipmentData.Values)
            {
                if (itemsToEquip.TryGetValue(slotData.itemId, out Item itemToEquip))
                {
                    TryEquip(itemToEquip as IEquipable);
                }
                else
                {
                    Debug.LogWarning($"Could not find item with ID {slotData.itemId} in inventory for equipping");
                }
            }*/
        }
    }
}