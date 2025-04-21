using UnityEngine;

public class SystemObserver : MonoBehaviour
{
    [SerializeField] int timesHit;
    [SerializeField] float timeSurvived, timeSurvived5, timeSurvived25, timeSurvived50;

    private void Update()
    {
        Timer(timeSurvived);
        if (GameManager.instance.player.GetAttributeContainer().attributes["Health"].CurrentValue()
            / GameManager.instance.player.GetAttributeContainer().attributes["MaxHealth"].CurrentValue() <= 0.05f)
            Timer(timeSurvived5);
    }

    private void Timer(float timer)
    {
        timer += Time.deltaTime;
    }
}