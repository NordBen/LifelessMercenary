using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemoIntAttributeText : MonoBehaviour
{
    public Button incButton, decButton;
    public TextMeshProUGUI statText;
    public TempPlayerStats attribute;

    void Start()
    {
        incButton.onClick.AddListener(() => UpdateStat(1));
        decButton.onClick.AddListener(() => UpdateStat(-1));
    }

    private void UpdateStat(int updAmount)
    {
        if (TempPlayerAttributes.instance.HasPointsToUse())
        {
            TestingUpgradePart.instance.DecreasePointsToUse(updAmount);
            TempPlayerAttributes.instance.tempPlayerIntAttributes[attribute] += updAmount;
            TempPlayerAttributes.instance.UpdateStats();
            this.statText.text = TempPlayerAttributes.instance.GetIntAttribute(attribute).ToString();
        }
    }
}