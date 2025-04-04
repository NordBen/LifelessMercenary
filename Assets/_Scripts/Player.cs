using UnityEngine;

public class Player : MonoBehaviour
{
    private EquipmentManager equipmentManager;
    private InventoryManager inventoryManager;
    private AttributeContainer attributeContainer;

    void Awake()
    {
        equipmentManager = GetComponent<EquipmentManager>();
        inventoryManager = GetComponent<InventoryManager>();
        attributeContainer = GetComponent<AttributeContainer>();
    }

    public EquipmentManager GetEquipmentManager()
    {
        return equipmentManager;
    }

    public InventoryManager GetInventoryManager()
    {
        return inventoryManager;
    }

    public AttributeContainer GetAttributeContainer()
    {
        return attributeContainer;
    }
}