using TMPro;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class UIValueTextElement : ExtendedUIElement
{
    [SerializeField] protected string additionalText = "";

    protected TMP_Text labelText;
    protected int _value, _otherValue;
    protected string labelString = "";

    private void Start()
    {
        labelText = GetComponent<TMP_Text>();
        UpdateTextWithValue(this.owner.GetLevel(), this.owner.GetLevel()); // initiates the UIText to Max allowed healing items of owning character
    }

    private void OnEnable()
    {
        //owner.OnHealingUsed += UpdateTextUI;
    }

    private void OnDisable()
    {
        //owner.OnHealingUsed -= UpdateTextUI;
    }

    protected void UpdateTextWithValue(int newValue)
    {
        this._value = newValue;
        labelString = $"{additionalText} {this._value}";
        labelText.text = labelString;
    }

    protected void UpdateTextWithValue(int newValue, int additionalValue)
    {
        this._value = newValue;
        this._otherValue = additionalValue;
        labelString = $"{additionalText} {this._value} / {this._otherValue}";
        labelText.text = labelString;
    }
}