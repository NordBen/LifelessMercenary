using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TempStartAgaion : MonoBehaviour
{
    [SerializeField] protected float targetTime = 35;
    [SerializeField] private float elapsedTime = 0;
    [SerializeField] private GameObject charon;

    private void Update()
    {
        if (elapsedTime >= targetTime)
        {
            elapsedTime = 0;
            GameManager.instance.ResetLoop();
        }
        elapsedTime += Time.deltaTime;
        Debug.Log($"elapsed time: {elapsedTime}");
    }
}