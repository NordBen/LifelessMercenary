using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ValueProgressBar : UIValueTextElement
{
    [SerializeField] protected Color barColor = Color.white;
    private Slider _progressBar;
    private Image _barFill;

    private void Start()
    {
        _progressBar = GetComponent<Slider>();
        _barFill = transform.GetChild(1).GetComponent<Image>();
        labelText = transform.GetChild(2).GetComponent<TMP_Text>();
        Invoke("InitProgressBar", .05f);
    }

    private void OnEnable()
    {
        owner.OnHPChanged += UpdateProgressBar; // subscribes to HPChange event of owner
    }

    private void OnDisable()
    {
        owner.OnHPChanged -= UpdateProgressBar; // unsubscribes from HPChange event of owner
    }

    private void InitProgressBar()
    {
        UpdateProgressBar(owner.GetMaxHP());
    }

    private void UpdateProgressBar(float newValue)
    {
        UpdateTextWithValue((int)newValue);
        //Debug.Log("value: " + this._value);

        if (this._otherValue <= 0)
            this._otherValue = (int)owner.GetMaxHP();
        //Debug.Log("other: " + this._otherValue);
        //Debug.Log("max: " + (int)owner.GetMaxHP() + "max float: " + owner.GetMaxHP());

        StartCoroutine(SmoothTransitionSlider());
    }

    private void UpdateProgressBar(float newValue, float newMaxValue)
    {
        UpdateTextWithValue((int)newValue, (int)newMaxValue);
        StartCoroutine(SmoothTransitionSlider());
    }

    private IEnumerator SmoothTransitionSlider()
    {
        // only calculates the percent value of current hp compared to max hp of owning character,
        // compares slider value to targetted value and changes the slider smoothly moving it towards the target value
        // with an error margin of 0.01 value difference
        float targetValue = this._value / this._otherValue;
        while (Mathf.Abs(_progressBar.value - targetValue) > 0.01f)
        {
            _progressBar.value = Mathf.MoveTowards(_progressBar.value, targetValue, Time.deltaTime * 0.5f);
            yield return null;
        }
        _progressBar.value = targetValue;
    }
}