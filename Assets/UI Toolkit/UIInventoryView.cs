using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class UIInventoryView : TestStorageView
{
    [SerializeField] string panelName = "Inventory";

    public override IEnumerator InitializeView(int size = 20)
    {
        slots = new Slot[size];
        root = _document.rootVisualElement;
        root.Clear();

        root.styleSheets.Add(_styleSheet);

        container = root.CreateChild("Container");

        var inventory = container.CreateChild("inventory");
        inventory.CreateChild("inventoryFrame");
        inventory.CreateChild("inventoryHeader").Add(new Label(panelName));

        var slotsContainer = inventory.CreateChild("slotsContainer");
        for (int i = 0; i < size; i++)
        {
            var slot = slotsContainer.CreateChild<Slot>("slot");
            slots[i] = slot;
        }
        
        var button = container.CreateChild<Button>("btn");

        yield return null;
    }
}