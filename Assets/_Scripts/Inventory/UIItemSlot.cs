using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    [SerializeField] protected Item referenceItem;
    protected Button _button;
    protected Image _icon;
    private TMP_Text _stackAmount;

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
}