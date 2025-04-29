using TMPro;
using UnityEngine;

public class TempDayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayCounterLabel;
    [SerializeField] private string dayString = "";

    void Start()
    {
        UpdateDayCounter(GameManager.instance.GetSurvivedDays());
    }
    
    private void OnEnable()
    {
        GameManager.instance.OnDaySurvived += UpdateDayCounter;
        Debug.Log("Subscribed to OnDaySurvived");
    }

    private void OnDisable()
    {
        GameManager.instance.OnDaySurvived -= UpdateDayCounter;
        Debug.Log("Unsubbed to OnDaySurvived");
    }

    private void UpdateDayCounter(int days)
    {
        dayCounterLabel.text = dayString + days;
        Debug.Log($"Updatedd day {dayString + days}");
    }
}