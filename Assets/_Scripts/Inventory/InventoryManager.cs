using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] protected List<Item> items;
    [SerializeField] private GameObject uiItem;

    [SerializeField] public GameObject inventoryScreen;
    [SerializeField] public GameObject inventoryContainer;

    public void ToggleInventory()
    {
        inventoryScreen.SetActive(!inventoryScreen.activeSelf);
        Debug.Log(inventoryScreen.activeSelf);
        //Cursor.lockState = inventoryScreen.activeSelf ? CursorLockMode.None : CursorLockMode.Locked;
        //Cursor.visible = inventoryScreen.activeSelf;
        //GetComponent<StarterAssetsInputs>().cursorLocked = !inventoryScreen.activeSelf;
        //GetComponent<StarterAssetsInputs>().cursorInputForLook = !inventoryScreen.activeSelf;
        /*if (inventoryScreen.activeSelf == false)
            Destroy(inventoryScreen.transform.GetChild(0));*/
    }

    public void ToggleInventoryWithoutType()
    {
        ToggleInventory();
        SortInventory(EItemType.All);
    }

    public void ToggleInventoryByType(EEquipSlot slot)
    {
        ToggleInventory();
        SortInventory(GetTypeForSlot(slot));
    }

    private EItemType GetTypeForSlot(EEquipSlot slot)
    {
        EItemType itemType;
        switch (slot)
        {
            case EEquipSlot.None:
                itemType = EItemType.All;
                break;
            case EEquipSlot.Quickslot:
                itemType = EItemType.Basic;
                break;
            case EEquipSlot.Head:
            case EEquipSlot.Torso:
            case EEquipSlot.Leggs:
            case EEquipSlot.Boots:
            case EEquipSlot.Amulet:
                itemType = EItemType.Armor;
                break;
            case EEquipSlot.Weapon:
                itemType = EItemType.Weapon;
                break;
            default:
                itemType = EItemType.All;
                break;
        }
        return itemType;
    }

    public void SortInventory(EItemType type)
    {
        if (inventoryScreen.transform.childCount != 0)
        {
            for (int i = 0; i > inventoryScreen.transform.childCount; i++)
            {
                Destroy(inventoryScreen.transform.GetChild(i));
            }
        }     
        
        GameObject inventoryGrid = Instantiate(inventoryContainer);
        inventoryGrid.transform.SetParent(inventoryScreen.transform, false);
        foreach (Item item in items)
        {
            if (item.type == type || type == EItemType.All)
            {
                GameObject itemInUI = Instantiate(uiItem);
                itemInUI.GetComponent<UIItemSlot>().SetReferenceItem(item);
                itemInUI.transform.SetParent(inventoryGrid.transform, false);
            }
        }
    }

    public void AddItem(Item inItem)
    {
        if (!items.Contains(inItem))
        {
            items.Add(inItem);
        }
    }

    public void AddItem(List<Item> listOfItemsToAdd)
    {
        foreach (Item itemToAdd in listOfItemsToAdd)
        {
            if (!items.Contains(itemToAdd))
            {
                items.Add(itemToAdd);
            }
        }
    }

    public void RemoveItem(Item inItem)
    {
        if (items.Contains(inItem))
        {
            items.Remove(inItem);
        }
    }

    public Item GetItem()
    {
        return items[0];
    }

    public Item GetItem(int index)
    {
        return items[index];
    }

    public List<Item> GetAllItems()
    {
        List<Item> allItems = this.items;
        return allItems;
    }

    public List<Item> GetAllItems(EItemType itemType)
    {
        List<Item> allItemsByType = new List<Item>();
        foreach (Item item in this.items)
        {
            if (item.type == itemType)
            {
                allItemsByType.Add(item);
            }
        }
        return allItemsByType;
    }
}