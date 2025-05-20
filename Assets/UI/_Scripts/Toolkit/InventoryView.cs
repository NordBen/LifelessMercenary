using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace LM.Inventory
{
    public class InventoryView : UIView
    {
        public static readonly string[] GradeKeys = { "All", "Common", "Rare", "Special" };
        public static readonly string[] SlotTypeKeys = { "All", "Weapon", "Shield", "Helmet", "Boots", "Gloves" };
        
        private ScrollView m_ScrollViewParent;

        private VisualElement m_InventoryBackButton;
        private VisualElement m_InventoryPanel;

        private DropdownField m_DropDown;
        
        private VisualTreeAsset m_ItemAsset;
        private UIItem m_selectedItem;

        public InventoryView(VisualElement topElement) : base(topElement)
        {
            InventoryEvents.ItemClicked += OnItemClicked;
            InventoryEvents.InventoryUpdated += OnInventoryUpdated;
            
            m_ItemAsset = Resources.Load("Item") as VisualTreeAsset;
        }

        protected override void SetVisualElements()
        {
            base.SetVisualElements();
            
            m_InventoryBackButton = m_TopElement.Q("inventory__back-button");
            m_InventoryPanel = m_TopElement.Q("inventory__screen");
            m_DropDown = m_TopElement.Q<DropdownField>("inventory__test-dropdown");
            
            m_ScrollViewParent = m_TopElement.Q<ScrollView>("inventory__scrollview");
        }

        public override void Dispose()
        {
            base.Dispose();
            InventoryEvents.ItemClicked -= OnItemClicked;
            InventoryEvents.InventoryUpdated -= OnInventoryUpdated;
            
            UnregisterButtonCallbacks();
        }

        protected override void RegisterButtonCallbacks()
        {
            m_DropDown.RegisterValueChangedCallback(UpdateFilters);
        }
        
        protected void UnregisterButtonCallbacks()
        {
            m_DropDown.UnregisterValueChangedCallback(UpdateFilters);
        }

        private EItemGrade GetGrade(string gradeString)
        {
            EItemGrade itemGrade = EItemGrade.Common;

            if (!Enum.TryParse<EItemGrade>(gradeString, out itemGrade))
            {
                Debug.LogError("Invalid grade: " + gradeString);
            }
            return itemGrade;
        }

        private EItemType GetItemType(string itemTypeString)
        {
            EItemType itemType = EItemType.All;
            
            if (!Enum.TryParse<EItemType>(itemTypeString, out itemType))
            {
                Debug.LogError("Invalid grade: " + itemTypeString);
            }
            return itemType;
        }
        
        private void UpdateFilters(ChangeEvent<string> evt)
        {
            string itemTypeKey = SlotTypeKeys[m_DropDown.index];
            string itemGradeKey = GradeKeys[m_DropDown.index];
            
            EItemType itemType = GetItemType(itemTypeKey);
            EItemGrade itemGrade = GetGrade(itemGradeKey);
            
            InventoryEvents.ItemsFiltered?.Invoke(itemType, itemGrade);
        }

        private void ShowItems(List<Item> itemsToShow)
        {
            VisualElement contentContainer = m_ScrollViewParent.Q<VisualElement>("unity-content-container");
            contentContainer.Clear();

            for (int i = 0; i < itemsToShow.Count; i++)
            {
                CreateItemButton(itemsToShow[i], contentContainer);
            }
        }

        private void CreateItemButton(Item itemData, VisualElement container)
        {
            if (container == null)
            {
                Debug.LogError("Container is null");
                return;
            }
            
            TemplateContainer itemUIElement = m_ItemAsset.Instantiate();
            itemUIElement.AddToClassList("item-spacing");

            UIItem itemSlot = new UIItem(itemData);
            
            itemSlot.SetVisualElements(itemUIElement);
            itemSlot.SetGameData(itemUIElement);
            itemSlot.RegisterButtonCallbacks();
            
            container.Add(itemUIElement);
        }

        private void SelectItem(UIItem item, bool select)
        {
            if (item == null) return;
            
            m_selectedItem = (select) ? item : null;
            item.CheckItem(select);
        }

        public override void Show()
        {
            base.Show();
            
            InventoryEvents.ScreenEnabled?.Invoke();
            UpdateFilters(null);

            m_InventoryPanel.transform.scale = new Vector3(0.1f, .1f, .1f);
            m_InventoryPanel.experimental.animation.Scale(1f, 200);
        }

        private void CloseWindow(ClickEvent evt)
        {
            Hide();
        }

        public override void Hide()
        {
            base.Hide();

            AudioManager.PlayDefaultButtonSound();

            if (m_selectedItem != null)
            {
                InventoryEvents.ItemSelected?.Invoke(m_selectedItem.ItemReference);
            }
            m_selectedItem = null;
        }

        private void OnInventorySetup()
        {
            SetVisualElements();
            RegisterButtonCallbacks();
        }

        private void OnInventoryUpdated(List<Item> itemsToLoad)
        {
            ShowItems(itemsToLoad);
        }
        
        private void OnItemClicked(UIItem item)
        {
            //AudioManager.PlayAltButtonSound();
            SelectItem(m_selectedItem, false);
            SelectItem(item, true);
        }
    }
}