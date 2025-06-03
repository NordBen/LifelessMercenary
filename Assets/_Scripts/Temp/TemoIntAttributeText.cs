using LM;
using LM.AbilitySystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemoIntAttributeText : MonoBehaviour
{
    public Button incButton, decButton;
    public TextMeshProUGUI statText;
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
        UpdateStatText();
    }

    private void UpdateStat(int updAmount)
    {
        if (GAC != null && GameManager.instance.player.GetComponent<LM.PlayerController>().StatPoints > 0)
        {
            var UpgradeEffect = EffectFactory.CreateAttributeUpgradeEffect(attributeToUpgrade, updAmount);
            GAC.ApplyEffect(UpgradeEffect, true);
            TestingUpgradePart.instance.DecreasePointsToUse(-updAmount);
            UpdateStatText();
                GAC.UpdateDerivedAttributes();
                GAC.ApplyEffect(GAC._fullHealEffect, false);
        }/*
        if (TempPlayerAttributes.instance.HasPointsToUse())
        {
            TestingUpgradePart.instance.DecreasePointsToUse(updAmount);
            TempPlayerAttributes.instance.tempPlayerIntAttributes[attribute] += updAmount;
            TempPlayerAttributes.instance.UpdateStats();
            this.statText.text = TempPlayerAttributes.instance.GetIntAttribute(attribute).ToString();
        }*/
    }

    private void UpdateStatText()
    {
        this.statText.text = GAC.GetRuntimeAttribute(attributeToUpgrade).CurrentValue.ToString();
    }
}