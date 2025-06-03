using System.Collections.Generic;
using LM.Inventory;
using UnityEngine;

[System.Serializable]
public class SaveGameData
{
    public long lastUpdated;
    public int deathCount = 0;
    public Vector3 playerPosition;
    public SerializableSaveDictionary<string, bool> coinsCollected;
    public int daysSurvived = 0;
    public int deathEnergy = 5;
    public int statPoints = 5;

    public float volume = 1;
    public float musicVolume = 1;
    public float sfxVolume = 1;
    public float ambientVolume = 1;
    public Vector2 mouseSensitivity = new Vector2(1, 1);

    public SerializableSaveDictionary<string, SerializableSaveEffect> savedEffects;
    public SerializableSaveDictionary<string, SerializableSaveAttribue> savedAttributes;//testCachedAttribute;
    public List<Item> savedItems;
    public Item[] savedEquipped;
    
    public SerializableSaveDictionary<string, SerializableItemList> savedInventory;
    public SerializableSaveDictionary<string, SerializableItemList> savedEquipment;
    
    //public SerializableSaveDictionary<string, List<SerializableAttributeData>> savedAttributes;// = new SerializableSaveDictionary<string, List<SerializableAttributeData>>();
    //public SerializableSaveDictionary<string, List<SerializableEffectData>> savedEffects;// = new SerializableSaveDictionary<string, List<SerializableEffectData>>();

    // the values defined in this constructor will be the default values
    // the game starts with when there's no data to load
    public SaveGameData()
    {
        deathCount = 0;
        playerPosition = Vector3.zero;
        coinsCollected = new SerializableSaveDictionary<string, bool>();
        savedEffects = new SerializableSaveDictionary<string, SerializableSaveEffect>();
        savedAttributes = new SerializableSaveDictionary<string, SerializableSaveAttribue>(); // testCachedAttribute;
        /*testCachedAttribute.Add("teli", testSaveAttribute);
        testCachedAttribute["teli"].attributes.Add(tesiAttribute);
        testCachedAttribute["teli"].attributes.Add(itsetAttribute);
        testCachedAttribute["teli"].attributes.Add(bopAttribute);*/
        /*testSaveAttribute = new SerializableSaveAttribue();
        testSaveAttribute.attributes.Add(tesiAttribute);
        testSaveAttribute.attributes.Add(itsetAttribute);
        testSaveAttribute.attributes.Add(bopAttribute);*/
        savedItems = new List<Item>();
        savedEquipped = new Item[6];
        
        savedInventory = new SerializableSaveDictionary<string, SerializableItemList>();
        savedEquipment = new SerializableSaveDictionary<string, SerializableItemList>();
    }

    public int GetPercentageComplete() 
    {
        // figure out how many coins we've collected
        int totalCollected = 0;
        foreach (bool collected in coinsCollected.Values) 
        {
            if (collected) 
            {
                totalCollected++;
            }
        }

        // ensure we don't divide by 0 when calculating the percentage
        int percentageCompleted = -1;
        if (coinsCollected.Count != 0) 
        {
            percentageCompleted = (totalCollected * 100 / coinsCollected.Count);
        }
        return percentageCompleted;
    }
}