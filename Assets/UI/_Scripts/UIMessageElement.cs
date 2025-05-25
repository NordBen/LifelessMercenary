using System.Collections;
using LM.Inventory;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class UIMessageElement : MonoBehaviour
{
    [Header("===References===")]
    [SerializeField] private TMPro.TextMeshProUGUI _text;
    [SerializeField] private Image _image;
    [Header("===Settings===")]
    [SerializeField] private string template = "Added {q} {i} to Inventory";
    [SerializeField] private float timeToLeave = 5;
    
    public IEnumerator Init(Item item, int quantity, Transform parent)
    {
        this.UpdateMessage(item, quantity);
        this.gameObject.transform.SetParent(parent, false);
        this.gameObject.transform.localPosition = Vector3.zero;
        yield return new WaitForSeconds(timeToLeave);
        Destroy(this.gameObject);
    }
    
    private void UpdateMessage(Item item, int quantity)
    {
        string quantityString = quantity > 1 ? "x" + quantity : null;
        string itemString = item.itemName;
        this._text.text = template.Replace("{q}", quantityString).Replace("{i}", itemString);
        this._image.sprite = item.icon;
    }
}