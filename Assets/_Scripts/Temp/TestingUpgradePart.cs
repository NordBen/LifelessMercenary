using TMPro;
using UnityEngine;

public class TestingUpgradePart : MonoBehaviour
{
    public static TestingUpgradePart instance;
    public TextMeshProUGUI pointsText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        pointsText.text = TempPlayerAttributes.instance.pointsToUse.ToString();
    }

    public void DecreasePointsToUse(int decAmount)
    {
        TempPlayerAttributes.instance.pointsToUse -= decAmount;
        pointsText.text = TempPlayerAttributes.instance.pointsToUse.ToString();
    }
}