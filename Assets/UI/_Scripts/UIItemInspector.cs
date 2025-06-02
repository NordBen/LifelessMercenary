using System;
using LM.Inventory;
using UnityEngine;
using UnityEngine.UI;

public class UIItemInspector : MonoBehaviour
{
    public static UIItemInspector Instance;
    private Item m_ReferenceItem;
    [SerializeField] private Image m_ItemImage;
    [SerializeField] private TMPro.TextMeshProUGUI m_ItemName;
    [SerializeField] private TMPro.TextMeshProUGUI m_ItemDescription;
    [SerializeField] private TMPro.TextMeshProUGUI m_ItemQuantity;
    [SerializeField] private TMPro.TextMeshProUGUI m_ItemWeight;
    [SerializeField] private TMPro.TextMeshProUGUI m_ItemValue;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        UIItemSlot.OnItemHovered += UpdateInspection;
    }

    private void OnDisable()
    {
        UIItemSlot.OnItemHovered -= UpdateInspection;
    }

    public void UpdateInspection(UIItemSlot uiItem)
    {
        m_ItemName.text = uiItem != null ? uiItem.RefItem.itemName : String.Empty;
        m_ItemImage.sprite = uiItem != null ? uiItem.RefItem.icon : null;
        m_ItemQuantity.text = uiItem != null ? uiItem.RefItem.quantity.ToString() : String.Empty;
    }
}