using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class TempPlayerHealthBar : MonoBehaviour
{
    public Slider healthSlider, staminaSlider;
    public TextMeshProUGUI healthPercentText, staminaPercentText;

    private void Awake()
    {
        TempPlayerAttributes.instance.OnHealthChanged += UpdateHealthBar;
        TempPlayerAttributes.instance.OnStaminaChanged += UpdateStaminaBar;
    }

    private void Start()
    {
        UpdateHealthBar(TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.health));
        UpdateStaminaBar(TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.stamina));
    }

    private void OnDisable()
    {
        TempPlayerAttributes.instance.OnHealthChanged -= UpdateHealthBar;
        TempPlayerAttributes.instance.OnStaminaChanged -= UpdateStaminaBar;
    }

    private void UpdateHealthBar(float newValue)
    {
        healthSlider.value = newValue / TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.maxhealth);
        healthPercentText.text = $"{TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.health)} / {TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.maxhealth)}";
    }

    private void UpdateStaminaBar(float newValue)
    {
        staminaSlider.value = newValue / TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.maxstamina);
        staminaPercentText.text = $"{TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.stamina)} / {TempPlayerAttributes.instance.GetFloatAttribute(TempPlayerStats.maxstamina)}";
    }
}