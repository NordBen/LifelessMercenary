using TMPro;
using UnityEngine;

public class AttributeText : MonoBehaviour
{
    [SerializeField] string bindedAttribute;
    [SerializeField] Attribute refAttri;
    [SerializeField] float value;
    [SerializeField] TextMeshProUGUI attributeText;

    private void Start()
    {
        refAttri = GameManager.instance.player.GetComponent<AttributeContainer>().attributes[bindedAttribute];

        if (refAttri != null)
        {
            refAttri.OnValueChanged += UpdateValue;
        }
    }

    private void OnDisable()
    {
        if (refAttri != null)
        {
            refAttri.OnValueChanged -= UpdateValue;
        }
    }

    void UpdateValue(float newValue, float oldValue)
    {
        this.value = newValue;
        UpdateUI();
    }

    void UpdateUI()
    {
        attributeText.text = value.ToString();
    }
}