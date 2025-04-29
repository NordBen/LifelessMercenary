using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipSlot : MonoBehaviour
{
    [SerializeField] private EEquipSlot _slot;
    [SerializeField] private bool isInteractable;
    [SerializeField] Sprite slotIcon;
    [SerializeField] private Item _slottedItem;
    private Image _slotIcon;
    private Image _equippedIcon;
    private Button _button;

    private void Start()
    {
        _button = transform.GetChild(0).GetChild(0).GetComponent<Button>();
        _slotIcon = transform.GetChild(0).GetChild(1).GetComponent<Image>();
        _equippedIcon = transform.GetChild(0).GetChild(3).GetComponent<Image>();
        _button.interactable = isInteractable;
        _button.onClick.AddListener(() => OnClick());
        _slotIcon.sprite = slotIcon;
        UpdateEquipmentSlot(null);
    }

    private void OnEnable()
    {
        Invoke("SubscribeToOnEquip", .5f);
    }

    private void OnDisable()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip -= UpdateEquipmentSlot;
    }

    private void SubscribeToOnEquip()
    {
        GameManager.instance.player.GetEquipmentManager().OnEquip += UpdateEquipmentSlot; 
    }

    public void UpdateEquipmentSlot(IEquipable newItem)
    {
        Debug.Log("HEYO UPDATEEQuipment UI");
        if (newItem != null)
        {
            if (newItem.GetSlot() != this._slot) return;
            else
            {
                Debug.Log("Got past Slot");
                if ((Item)newItem == this._slottedItem)
                {
                    Debug.Log("New item is same item");
                    this._slottedItem = null;
                    Color colorTransparancy = this._slotIcon.color;
                    colorTransparancy.a = 0;
                    this._equippedIcon.color = colorTransparancy;
                    this._equippedIcon.sprite = null;
                }
                else
                {
                    Debug.Log("New item is new");
                    this._slottedItem = (Item)newItem;
                    Color colorTransparancy = this._slotIcon.color;
                    colorTransparancy.a = 1;
                    this._equippedIcon.color = colorTransparancy;
                    this._equippedIcon.sprite = _slottedItem.icon;
                }
            }
        }
        else
        {
            Debug.Log("New item is null");
            this._slottedItem = null;
            Color colorTransparancy = this._slotIcon.color;
            colorTransparancy.a = 0;
            this._equippedIcon.color = colorTransparancy;
            this._equippedIcon.sprite = null;
        }
    }

    protected void OnClick()
    {
        //Debug.Log($"Called on click for slot {this._slot}");
        GameManager.instance.player.GetInventoryManager().ToggleInventoryByType(this._slot);
    }
}