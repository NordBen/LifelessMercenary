using TMPro;
using UnityEngine;

public class AttributeText : MonoBehaviour
{
    [SerializeField] protected string bindedAttribute;
    [SerializeField] protected Attribute refAttri;
    [SerializeField] protected float value;
    [SerializeField] protected TextMeshProUGUI attributeText;

    protected void Start()
    {
        refAttri = GameManager.instance.player.GetComponent<AttributeContainer>().attributes[bindedAttribute];

        if (refAttri != null)
        {
            refAttri.OnValueChanged += UpdateValue;
        }

        UpdateValue(refAttri.CurrentValue(), refAttri.BaseValue());
    }

    protected void OnDisable()
    {
        if (refAttri != null)
        {
            refAttri.OnValueChanged -= UpdateValue;
        }
    }

    private void UpdateValue(float newValue, float oldValue)
    {
        this.value = newValue;
        UpdateUI();
    }

    private void UpdateUI()
    {
        attributeText.text = value.ToString();
    }
}