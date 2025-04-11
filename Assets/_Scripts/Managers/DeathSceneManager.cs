using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeathSceneManager : MonoBehaviour
{
    [SerializeField] protected float targetTime = 5;
    [SerializeField] private float elapsedTime = 0;

    [SerializeField] TextMeshProUGUI deathText;
    [SerializeField] TextMeshProUGUI deText;
    [SerializeField] Slider deathSlider;

    void Start()
    {
        GameManager.instance.OnDeathEnergyChanged += UpdateDEText;
        UpdateDEText(GameManager.instance.GetDeEnergt());
        Debug.Log(GameManager.instance.GetDeEnergt());
    }

    private void OnDisable()
    {
        GameManager.instance.OnDeathEnergyChanged -= UpdateDEText;
    }

    void Update()
    {
        if (elapsedTime >= targetTime)
        {
            elapsedTime = 0;
            GameManager.instance.ResetLoop();
        }
        elapsedTime += Time.deltaTime;
    }

    private void UpdateDEText(int newValue)
    {
        deText.text = $"{newValue} / {GameManager.instance.GetMaxDeEnerg()}";
        deathSlider.value = (float)newValue / GameManager.instance.GetMaxDeEnerg();
        Debug.Log($"{(float)newValue / GameManager.instance.GetMaxDeEnerg()}");
    }
}
