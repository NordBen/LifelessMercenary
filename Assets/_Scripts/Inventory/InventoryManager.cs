using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LM.Inventory
{
    public class InventoryManager : MonoBehaviour //, IDataPersistance
    {
        [SerializeField] private string m_ResourcePath = "GameData/Items";

        [SerializeField] protected List<Item> items;
        [SerializeField] private GameObject uiItem;

        [SerializeField] public GameObject inventoryScreen;
        [SerializeField] public GameObject inventoryContainer;

        private List<Item> equippedItems;
        private List<Item> unequippedItems;

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

        public List<Item> HeldItems => this.items;

        private void OnEnable()
        {
            InventoryEvents.ScreenEnabled += OnInventoryScreenEnabled;
            InventoryEvents.ItemsFiltered += OnItemsFiltered;
            InventoryEvents.ItemSelected += OnItemEquipped;
        }

        private void OnDisable()
        {
            InventoryEvents.ScreenEnabled -= OnInventoryScreenEnabled;
            InventoryEvents.ItemsFiltered -= OnItemsFiltered;
            InventoryEvents.ItemSelected -= OnItemEquipped;
        }

        private void OnInventoryScreenEnabled()
        {
            UpdateInventory(unequippedItems);
        }

        private void FilterInventory(EItemType type, EItemGrade grade)
        {
            List<Item> filterItems = new List<Item>();
            filterItems = FilterItemList(items, grade, type);
            UpdateInventory(filterItems);
        }

        private void OnItemsFiltered(EItemType type, EItemGrade grade)
        {
            FilterInventory(type, grade);
        }

        private void OnItemEquipped(Item itemToEquip)
        {
            if (itemToEquip == null) return;

            unequippedItems.Remove(itemToEquip);
            equippedItems.Add(itemToEquip);
            InventoryEvents.InventoryUpdated?.Invoke(unequippedItems);
        }

        private void OnItemUnequipped(Item itemToEquip)
        {
            if (itemToEquip == null) return;

            unequippedItems.Add(itemToEquip);
            equippedItems.Remove(itemToEquip);
            unequippedItems = SortItemList(unequippedItems);
        }

        private void UpdateInventory(List<Item> listOfItems)
        {
            unequippedItems = SortItemList(unequippedItems);
            InventoryEvents.InventoryUpdated?.Invoke(listOfItems);
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
                if (item.itemType == type || type == EItemType.All)
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
                if (item.itemType == itemType)
                {
                    allItemsByType.Add(item);
                }
            }

            return allItemsByType;
        }

        public Item FindItemById(string itemId)
        {
            return items.Find(item => item.itemId == itemId);
        }

        public void SaveData(SaveGameData data)
        {
            List<SerializableItem> inventoryData = new List<SerializableItem>();

            foreach (var item in items)
            {
                if (item == null) continue;

                SerializableItem serializedItem = SerializableItem.CreateFrom(item);
                if (serializedItem != null)
                {
                    inventoryData.Add(serializedItem);
                }
            }

            SerializableItemList inventoryToSave = new SerializableItemList();
            inventoryToSave.items = inventoryData;
            data.savedInventory[gameObject.name] = inventoryToSave;
        }

        public void LoadData(SaveGameData data)
        {
            if (!data.savedInventory.ContainsKey(gameObject.name)) return;

            items.Clear();

            List<Item> itemsToLoad = new List<Item>();

            var inventoryData = data.savedInventory[gameObject.name];
            foreach (var savedItem in inventoryData.items)
            {
                if (savedItem == null) continue;

                Item newItem = ItemDatabase.instance.GetItemById(savedItem.itemId);
                if (newItem != null)
                {
                    //Item itemInstance = Instantiate(newItem);

                    //items.Add(newItem);
                    savedItem.ApplyTo(newItem);
                    itemsToLoad.Add(newItem);
                }
                else
                {
                    Debug.LogWarning($"Could not find item with ID {savedItem.itemId} in database");
                    continue;
                }
            }

            items = itemsToLoad;
        }

        private List<Item> FilterItemList(List<Item> listOfItems, EItemGrade grade = EItemGrade.All,
            EItemType type = EItemType.All)
        {
            List<Item> filteredItems = listOfItems;

            if (grade != EItemGrade.All)
            {
                filteredItems = filteredItems.Where(x => x.itemGrade == grade).ToList();
            }

            if (type != EItemType.All)
            {
                filteredItems = filteredItems.Where(x => x.itemType == type).ToList();
            }

            return filteredItems;
        }

        private List<Item> SortItemList(List<Item> listToSort)
        {
            if (listToSort.Count <= 1)
                return listToSort;

            return listToSort.OrderBy
                (x => x.itemType).ThenBy(x => x.itemGrade).ThenBy(x => x.name).ToList();
        }

        public Item GetBestByType(EItemType type, List<Item> listOfItems)
        {
            Item itemToReturn = null;
            foreach (var item in listOfItems)
            {
                if (item.itemType != type)
                    continue;

                if (itemToReturn == null || (int)item.itemGrade > (int)itemToReturn.itemGrade)
                {
                    itemToReturn = item;
                }
            }

            return itemToReturn;
        }

        private List<EItemType> GetUnusedTypes(InventoryManager inventory)
        {
            List<EItemType> unusedTypes = new List<EItemType>() { EItemType.Weapon, EItemType.Armor };
            for (int i = 0; i < inventory.HeldItems.Count; i++)
            {
                if (inventory.HeldItems[i] != null)
                {
                    unusedTypes.Remove(inventory.HeldItems[i].itemType);
                }
            }

            return unusedTypes;
        }

        private List<Item> GetUnusedItems(InventoryManager inventory)
        {
            List<Item> unusedItems = new List<Item>();
            List<EItemType> unusedTypes = GetUnusedTypes(inventory);

            int slotsToFill = inventory.HeldItems.Count(s => s == null);

            for (int i = 0; i < unusedTypes.Count; i++)
            {
                if (slotsToFill <= 0)
                    break;

                Item nextItem = GetBestByType(unusedTypes[i], unequippedItems);
                if (nextItem != null)
                {
                    unusedItems.Add(nextItem);
                    slotsToFill--;
                }
            }
            return unusedItems;
        }
    }
}