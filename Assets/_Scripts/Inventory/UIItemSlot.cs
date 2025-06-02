using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;

namespace LM.Inventory
{
    public class UIItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] protected Item referenceItem;
        protected Button _button;
        protected Image _icon;
        private TMP_Text _stackAmount;
        public Item RefItem => referenceItem;
        
        public static event Action<UIItemSlot> OnItemHovered;

        public void SetReferenceItem(Item inItem)
        {
            this.referenceItem = inItem;
            InstantiateUIItem();
        }

        public void InstantiateUIItem()
        {
            InitComponents();
            _icon.sprite = referenceItem.icon;
            QuantityAndColor();
        }

        public virtual void InitComponents()
        {
            _button = transform.GetChild(0).GetComponent<Button>();
            _icon = transform.GetChild(1).GetComponent<Image>();
            _stackAmount = transform.GetChild(2).GetComponent<TMP_Text>();

            _button.onClick.AddListener(() => OnClick());
        }

        private void QuantityAndColor()
        {
            _button.image.color = referenceItem.GetColorByItemGrade();
            if (referenceItem.bIsStackable && referenceItem.quantity > 0)
            {
                _stackAmount.text = "x" + referenceItem.quantity.ToString();
                _stackAmount.enabled = true;
            }
        }

        protected virtual void OnClick()
        {
            GameManager.instance.player.GetEquipmentManager().TryEquip((IEquipable)referenceItem);
            GameManager.instance.player.GetInventoryManager().ToggleInventory();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnItemHovered?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnItemHovered?.Invoke(null);
        }
    }

    public class UIItem
    {
        private Item referenceItem;
        private VisualElement m_checkMark;
        private VisualElement m_Icon;

        public Item ItemReference => referenceItem;
        public VisualElement Icon => m_Icon;

        public UIItem(Item itemData)
        {
            referenceItem = itemData;
        }

        public void SetVisualElements(TemplateContainer itemElement)
        {
            if (itemElement == null) return;

            m_Icon = itemElement.Q("item__Icon");
            m_checkMark = itemElement.Q("item__CheckMark");

            m_checkMark.style.display = DisplayStyle.None;
        }

        public void SetGameData(TemplateContainer gearElement)
        {
            if (gearElement == null) return;
            m_Icon.style.backgroundImage = new StyleBackground(referenceItem.icon);
        }

        public void RegisterButtonCallbacks()
        {
            m_Icon.RegisterCallback<ClickEvent>(OnClick);
        }

        private void OnClick(ClickEvent evt)
        {
            ToggleCheck();

            InventoryEvents.ItemClicked?.Invoke(this);
        }

        private void ToggleCheck()
        {
            if (m_checkMark == null)
                return;

            bool state = m_checkMark.style.display == DisplayStyle.None;
            CheckItem(state);
        }

        public void CheckItem(bool state)
        {
            if (m_checkMark == null)
                return;

            m_checkMark.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}