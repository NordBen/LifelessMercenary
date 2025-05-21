using System.Collections.Generic;
using UnityEngine;
using LM.AbilitySystem;

[System.Serializable]
public class SaveAttributesData
{
    
}

[System.Serializable]
public class SerializableSaveAttribue
{
    public List<SerializableAttributeData> attributes = new List<SerializableAttributeData>();
}

[System.Serializable]
public class SerializableSaveEffect
{
    public List<SerializableEffectData> effects = new List<SerializableEffectData>();
}

[System.Serializable]
public class SerializableAttributeData
{
    public string attributeName;
    public float baseValue;
}

[System.Serializable]
public class SerializableEffectData
{
    public string effectName;
    public EEffectDurationType durationType;
    public float duration;
    public float period;
    public EModifierOperationType modifierType;
    public List<SerializableEffectApplicationData> applications = new List<SerializableEffectApplicationData>();
    public SerializableObjectData source;
}

[System.Serializable]
public class SerializableEffectApplicationData
{
    public string targetAttributeName;
    public EModifierOperationType modifierType;
    public string valueStrategyType;
    public float constantValue;
    public string sourceAttributeName;
    public float coefficient;
}

[System.Serializable]
public class SerializableItemList
{
    public List<SerializableItem> items = new List<SerializableItem>();
}

[System.Serializable]
public class SerializableItem
{
    public string itemId;
    public EItemType itemType;
    public bool bIsStackable = false;
    public int quantity;

    public static SerializableItem CreateFrom(Item item)
    {
        if (item == null) return null;

        if (item is IEquipable)
        {
            return SerializableItemEquipable.CreateFrom(item as IEquipable);
        }/*
        else if (item is WeaponObject)
        {
            return SerializedWeaponItem.CreateFrom(item as WeaponObject);
        }*/

        return new SerializableItem
        {
            itemId = item.itemId,
            quantity = item.quantity
        };
    }

    public virtual void ApplyTo(Item item)
    {
        if (item == null) return;
        
        //item.SetItemId(itemId);
        item.quantity = quantity;
    }
}

[System.Serializable]
public class SerializableItemEquipable : SerializableItem
{
    public EEquipSlot equipSlot;
    
    public static SerializableItemEquipable CreateFrom(IEquipable equipable)
    {
        if (equipable == null) return null;
        
        Item item = equipable as Item;
        if (item == null) return null;

        SerializableItemEquipable serialized = new SerializableItemEquipable
        {
            itemId = item.itemId,
            itemType = item.itemType,
            bIsStackable = item.bIsStackable,
            quantity = item.quantity,
            equipSlot = equipable.GetSlot()
        };
        
        return serialized;
    }

    public override void ApplyTo(Item item)
    {
        base.ApplyTo(item);

        if (item is IEquipable equipable)
        {
            //equipable.GetSlot() = equipSlot;
        }
    }
}

[System.Serializable]
public class SerializableObjectReference
{
    public string objectPath;
    public string objectType;

    public SerializableObjectReference(UnityEngine.Object obj)
    {
        if (obj == null)
        {
            objectPath = "";
            objectType = "";
            return;
        }
        
        if (obj is GameObject gameObj)
        {
            objectPath = GetGameObjectPath(gameObj);
            objectType = "GameObject";
        }
        else if (obj is Component component)
        {
            objectPath = GetGameObjectPath(component.gameObject);
            objectType = component.GetType().FullName;
        }
        else
        {
            objectPath = obj.name;
            objectType = obj.GetType().FullName;
        }
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    public UnityEngine.Object FindReferencedObject()
    {
        if (string.IsNullOrEmpty(objectPath) || string.IsNullOrEmpty(objectType))
            return null;

        if (objectType == "GameObject")
        {
            return GameObject.Find(objectPath);
        }
        else
        {
            GameObject obj = GameObject.Find(objectPath.Substring(0, objectPath.LastIndexOf('/')));
            if (obj != null)
            {
                System.Type componentType = System.Type.GetType(objectType);
                if (componentType != null)
                {
                    return obj.GetComponent(componentType);
                }
            }
        }
        return null;
    }
}

[System.Serializable]
public class SerializableObjectData
{
    public string objectType;
    public string objectId;
    
    public SerializableSaveDictionary<string, string> properties = new SerializableSaveDictionary<string, string>();
    
    public SerializableObjectData(object obj)
    {
        if (obj == null)
        {
            objectType = "";
            objectId = "";
            return;
        }
        
        objectType = obj.GetType().FullName;
        
        var idProperty = obj.GetType().GetProperty("Id") ??
                         obj.GetType().GetProperty("ID") ??
                         obj.GetType().GetProperty("UniqueId");

        if (idProperty != null)
        {
            objectId = idProperty.GetValue(obj)?.ToString();
        }
        else
        {
            objectId = obj.GetHashCode().ToString();
        }

        StoreRelevantProperties(obj);
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;

        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    private void StoreRelevantProperties(object obj)
    {
        if (obj.GetType().Name == "GameplayEffect")
        {
            StoreProperty(obj, "effectName");
        }
    }

    private void StoreProperty(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        if (property != null)
        {
            var value = property.GetValue(obj);
            if (value != null)
            {
                properties[propertyName] = value.ToString();
            }
        }
    }
}