using LM.Inventory;
using UnityEngine;

namespace LM
{
    public class Player : MonoBehaviour
    {
        private EquipmentManager equipmentManager;
        private InventoryManager inventoryManager;
        private AttributeContainer attributeContainer;
        private Animator animator;
        private CombatManager combatManager;

        void Awake()
        {
            equipmentManager = GetComponent<EquipmentManager>();
            inventoryManager = GetComponent<InventoryManager>();
            attributeContainer = GetComponent<AttributeContainer>();
            animator = GetComponent<Animator>();
            combatManager = GetComponent<CombatManager>();
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

        public Animator GetAnimator()
        {
            return animator;
        }

        public CombatManager GetCombatManager()
        {
            return combatManager;
        }

        public void Heal()
        {
            GameObject.Find("PlayerStats").GetComponent<TempPlayerAttributes>().ModifyHealth(GameObject
                .Find("PlayerStats").GetComponent<TempPlayerAttributes>().GetFloatAttribute(TempPlayerStats.maxhealth));
        }
    }
}