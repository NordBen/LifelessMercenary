using System;
using System.Collections;
using LM.AbilitySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct FBarStyle
{
    public Sprite fillImage;
    public Color barColor;
    public TMP_FontAsset labelFont;
    public float labelFontSize;
    public bool bOverrideFontSize;
    public Color labelColor;
}

public class UIAttributeValueProgressBar : MonoBehaviour
{
    [SerializeField] private GameplayAttributeComponent owner;
    [SerializeField] private GameplayAttribute referenceAttribute;
    [SerializeField] private GameplayAttribute referenceOtherAttribute;
    [SerializeField] private float maxBarLength = 1000f;
    [SerializeField] private float minBarLength = 200f;
    [SerializeField] private float barLength = 700f;
    [SerializeField] private bool bFlexibleLength = false;
    [SerializeField] private bool bExpandRight = true;
    [SerializeField] private FBarStyle style;
    [SerializeField] private TMP_Text _valueLabel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private Image _barFill;
    [SerializeField] private RectTransform _rectTransform;
    
    private float _attributeValue = 100;
    private float _otherAttributeValue = 100;
    private string _labelString;
    
    private void Start()
    {
        if (owner != null && referenceAttribute != null)
        {
            GameplayAttribute attributeToFind = owner.GetRuntimeAttribute(referenceAttribute);
            if (attributeToFind != null)
                attributeToFind.OnValueChanged += OnAttributeChanged;
            else
                Debug.Log("Failed to find attribute: " + referenceAttribute + "");
            
            GameplayAttribute otherAttributeToFind = owner.GetRuntimeAttribute(referenceOtherAttribute);
            if (otherAttributeToFind != null)
                otherAttributeToFind.OnValueChanged += OnOtherAttributeChanged;
            else 
                Debug.Log("Failed to find attribute: " + referenceOtherAttribute + "");
        }
        FetchComponents();
        AttributeInit(referenceAttribute);
    }
    
    private void OnDisable()
    {
        if (owner != null && referenceAttribute != null)
        {
            GameplayAttribute attributeToFind = owner.GetRuntimeAttribute(referenceAttribute);
            if (attributeToFind != null)
                attributeToFind.OnValueChanged -= OnAttributeChanged;
            
            GameplayAttribute otherAttributeToFind = owner.GetRuntimeAttribute(referenceOtherAttribute);
            if (otherAttributeToFind != null)
                otherAttributeToFind.OnValueChanged -= OnOtherAttributeChanged;
        }
    }
    
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        SetStyle();
    }

    void AttributeInit(GameplayAttribute a)
    {
        UpdateVisuals(a.CurrentValue, a.CurrentValue, 1.0f);
    }

    private void FetchComponents()
    {
        if (_progressBar == null)
            _progressBar = GetComponent<Slider>();
        if (_valueLabel == null)
            _valueLabel = transform.GetChild(2).GetComponent<TMP_Text>();
        if (_barFill == null)
            _barFill = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();
        
        SetStyle();
    }
    
    private void SetStyle()
    {
        if (_barFill.sprite != style.fillImage && style.fillImage != null) 
            _barFill.sprite = style.fillImage;
        
        if (_barFill.color != style.barColor && style.labelColor != Color.clear) 
            _barFill.color = style.barColor;
        
        if (_valueLabel.font != style.labelFont && style.labelFont != null)
            _valueLabel.font = style.labelFont;
        
        if (style.bOverrideFontSize && _valueLabel.fontSize != style.labelFontSize && style.labelFontSize > 0)
            _valueLabel.fontSize = style.labelFontSize;
        
        if (_valueLabel.color != style.labelColor && style.labelColor != Color.clear)
            _valueLabel.color = style.labelColor;
        
        if (_valueLabel.enableAutoSizing == style.bOverrideFontSize)
            _valueLabel.enableAutoSizing = !style.bOverrideFontSize;
        
        if (_rectTransform.rect.width != barLength && bFlexibleLength && (barLength >= minBarLength && barLength <= maxBarLength))
            ExpandRectWidth();
    }
    
    private void ExpandRectWidth()
    {
        float widthDifference = (barLength - _rectTransform.rect.width) * 0.5f;
        _rectTransform.sizeDelta = new Vector2(barLength, _rectTransform.sizeDelta.y);
        if (bExpandRight)
            _rectTransform.anchoredPosition += new Vector2(widthDifference, 0);
        else
            _rectTransform.anchoredPosition -= new Vector2(widthDifference, 0);
        //_rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 5, barLength);
        //_rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barLength);
    }
    
    public void Setup(GameplayAttributeComponent inGameplayAttributeComponent, GameplayAttribute inAttributeToReference, GameplayAttribute inOtherAttributeToReference)
    {
        if (owner != null && referenceAttribute != null)
        {
            GameplayAttribute attributeToFind = owner.GetRuntimeAttribute(referenceAttribute);
            if (attributeToFind != null)
                attributeToFind.OnValueChanged -= OnAttributeChanged;
            
            GameplayAttribute otherAttributeToFind = owner.GetRuntimeAttribute(referenceOtherAttribute);
            if (otherAttributeToFind != null)
                otherAttributeToFind.OnValueChanged -= OnOtherAttributeChanged;
        }

        owner = inGameplayAttributeComponent;
        referenceAttribute = inAttributeToReference;
        referenceOtherAttribute = inOtherAttributeToReference;

        if (owner != null && referenceAttribute != null)
        {
            GameplayAttribute attributeToFind = owner.GetRuntimeAttribute(referenceAttribute);
            if (attributeToFind != null)
                attributeToFind.OnValueChanged += OnAttributeChanged;

            GameplayAttribute otherAttributeToFind = owner.GetRuntimeAttribute(referenceOtherAttribute);
            if (otherAttributeToFind != null)
                otherAttributeToFind.OnValueChanged += OnOtherAttributeChanged;
        }
    }

    private void OnAttributeChanged(AttributeChangedEvent attribute)
    {
        this._attributeValue = attribute.newValue;
        float targetPercent = _attributeValue / _otherAttributeValue;
        UpdateVisuals(this._attributeValue, this._otherAttributeValue, targetPercent);
    }

    private void OnOtherAttributeChanged(AttributeChangedEvent attribute)
    {
        //this._progressBar;
        this._otherAttributeValue = attribute.newValue;
        float targetPercent = _attributeValue / _otherAttributeValue;
        UpdateVisuals(this._attributeValue, this._otherAttributeValue, targetPercent);
    }

    private void UpdateVisuals(float value, float otherValue, float targetPercent)
    {
        StartCoroutine(UpdateTextWithValueSlowed((int)value, (int)otherValue));//UpdateTextWithValue((int)value, (int)otherValue);
        StartCoroutine(SmoothTransitionSlider(targetPercent));
    }

    private void UpdateTextWithValue(float value, float otherValue)
    {
        _labelString = $"{value} / {otherValue}";
        _valueLabel.text = this._labelString;
    }
    
    private IEnumerator UpdateTextWithValueSlowed(float value, float otherValue)
    {
        yield return null;
        _labelString = $"{value} / {otherValue}";
        _valueLabel.text = this._labelString;
    }
    
    private void UpdateTextWithValue(float value)
    {
        _labelString = $"{value}";
        _valueLabel.text = this._labelString;
    }

    private IEnumerator SmoothTransitionSlider(float targetValue)
    {
        yield return null;
        float oldValue = this._progressBar.value;
        while (Mathf.Abs(this._progressBar.value - targetValue) > 0.01f)
        {
            this._progressBar.value = Mathf.MoveTowards(this._progressBar.value, targetValue, Time.deltaTime * 0.05f);
            yield return null;
        }
        this._progressBar.value = targetValue;
    }
}