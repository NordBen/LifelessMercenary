using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemoIntAttributeText : MonoBehaviour
{
    public Button incButton, decButton;
    public TextMeshProUGUI statText;
    public TempPlayerStats attribute;
    public GameplayAttribute attributeToUpgrade;

    private GameplayAttributeComponent GAC;

    void Start()
    {
        incButton.onClick.AddListener(() => UpdateStat(1));
        decButton.onClick.AddListener(() => UpdateStat(-1));

        if (GAC == null)
        {
            GAC = GameManager.instance.player.GetComponent<GameplayAttributeComponent>();
        }
    }

    private void UpdateStat(int updAmount)
    {
        if (GAC != null)
        {
            var UpgradeEffect = GameplayEffectFactory.CreateAttributeUpgradeEffect(attributeToUpgrade, updAmount);
            GAC.ApplyEffect(UpgradeEffect, true);
            this.statText.text = GAC.GetRuntimeAttribute(attributeToUpgrade).CurrentValue().ToString();
            //GAC.UpdateDerivedAttributes();
            
                GAC.UpdateDerivedAttributes();
        }/*
        if (TempPlayerAttributes.instance.HasPointsToUse())
        {
            TestingUpgradePart.instance.DecreasePointsToUse(updAmount);
            TempPlayerAttributes.instance.tempPlayerIntAttributes[attribute] += updAmount;
            TempPlayerAttributes.instance.UpdateStats();
            this.statText.text = TempPlayerAttributes.instance.GetIntAttribute(attribute).ToString();
        }*/
    }
}