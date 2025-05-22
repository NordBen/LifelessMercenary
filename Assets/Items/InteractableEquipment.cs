using UnityEngine;

namespace LM.Inventory
{
    public class InteractableEquipment : InteractableActor
    {
        [SerializeField] private Item item;

        private void Start()
        {
            this.SetMesh(item.mesh, item.material);
        }

        protected override void HandleInteract()
        {
            item.Interact();

            if (item is IEquipable equipableItem)
            {
                var slotType = equipableItem.GetSlot();

                var equipmentManager = GameManager.instance.player.GetEquipmentManager();

                if (equipmentManager.GetEquippedItem(slotType) == null)
                {
                    equipmentManager.TryEquip(equipableItem);
                }
            }
        }
    }
}