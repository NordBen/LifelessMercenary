using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;

    public event Action<int> OnDeathEnergyChanged;
    [SerializeField] private int deathEnergyMax = 5;
    [SerializeField] private int currentDeathEnergy = 5;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void ModifyDeathEnergy(int amount)
    {
        currentDeathEnergy += amount;
        OnDeathEnergyChanged?.Invoke(currentDeathEnergy);
        Debug.Log($"de: {currentDeathEnergy} / {deathEnergyMax}");
    }

    public int GetDeEnergt()
    {
        return currentDeathEnergy;
    }

    public int GetMaxDeEnerg()
    {
        return deathEnergyMax;
    }

    public void KillPlayer()
    {
        GameObject.Find("SceneDude").GetComponent<PlayableDirector>().Play();
        ModifyDeathEnergy(-1);

        Debug.Log($"dur to die: {(float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration}");
        Invoke("Death", (float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration);
    }

    public void Death()
    {
        SceneManager.LoadScene("DeathScene");
    }

    public void ResetLoop()
    {
        SceneManager.LoadScene(0);//"TheLevelScene");
    }
}
