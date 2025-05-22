using System.Collections.Generic;
using System.Linq;
using LM.Inventory;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase instance { get; private set; }
    
    [SerializeField] private List<Item> allItems;
    private Dictionary<string, Item> itemsByID;
    private Dictionary<string, Item> itemsByName;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeDatabase();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDatabase()
    {
        itemsByID.Clear();
        itemsByName.Clear();

        if (allItems.Count == 0)
        {
            allItems.AddRange(Resources.LoadAll<Item>("Items"));
            Debug.Log($"Loaded {allItems.Count} items from Resources folder");
        }

        foreach (var item in allItems)
        {
            if (item == null) continue;

            string itemId = item.itemId;

            if (!itemsByID.ContainsKey(itemId))
            {
                itemsByID.Add(itemId, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate item ID found: {itemId}");
            }
            
            if (!itemsByName.ContainsKey(item.itemName))
            {
                itemsByName.Add(item.itemName, item);
            }
            else
            {
                Debug.LogWarning($"Duplicate item name found: {item.itemName}");
            }
        }
        Debug.Log($"Item database initialized with {allItems.Count} items");
    }

    public Item GetItemById(string id)
    {
        if (itemsByID.TryGetValue(id, out Item item))
        {
            return Instantiate(item); //item.Clone();
        }
        Debug.LogWarning($"Item with ID {id} not found in database");
        return null;
    }
    
    public Item GetItemByName(string name)
    {
        if (itemsByName.TryGetValue(name, out Item item))
        {
            return Instantiate(item);//item.Clone();
        }
        Debug.LogWarning($"Item with name {name} not found in database");
        return null;
    }
    
    public List<Item> GetItemsByType(EItemType type)
    {
        return allItems.Where(item => item.itemType == type)
            .Select(item => Instantiate(item))
            .ToList();
    }
}