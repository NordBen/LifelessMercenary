using System;
using UnityEngine;

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
    }
}
