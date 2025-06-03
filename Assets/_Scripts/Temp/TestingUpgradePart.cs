using LM;
using TMPro;
using UnityEngine;

public class TestingUpgradePart : MonoBehaviour
{
    public static TestingUpgradePart instance;
    public TextMeshProUGUI pointsText;

    private LM.PlayerController player;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        player = GameManager.instance.player.GetComponent<LM.PlayerController>();
        pointsText.text = player.StatPoints.ToString();
    }

    public void DecreasePointsToUse(int decAmount)
    {
        player.ModifyStatPoints(decAmount);
        pointsText.text = player.StatPoints.ToString();
    }
}