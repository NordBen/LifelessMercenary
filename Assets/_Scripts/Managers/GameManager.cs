using StarterAssets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;

    public List<NPCController> enemies;

    public event Action<int> OnDeathEnergyChanged;
    [SerializeField] private int deathEnergyMax = 5;
    [SerializeField] private int currentDeathEnergy = 5;

    public event Action<int> OnDaySurvived;
    [SerializeField] private int daysSurvived = 1;

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
        if (player == null && GameObject.Find("Player"))
            player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (player == null && GameObject.Find("Player"))
            player = GameObject.Find("Player").GetComponent<Player>();
        TempPlayerAttributes.instance.SetPlayerController(GameObject.Find("Player").GetComponent<ThirdPersonController>());
        TempPlayerAttributes.instance.UpdateStats();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    void Start()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager_sceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainMenu"))
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ResetLoop();
            }
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            SurviveDay(1);
        }
    }

    public void SurviveDay(int days)
    {
        this.daysSurvived += days;
        OnDaySurvived?.Invoke(daysSurvived);
        Debug.Log($"days survived: {daysSurvived}");
    }
    
    public int GetSurvivedDays() => daysSurvived;

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
        SceneManager.LoadScene("TheLevelScene");
    }

    public void AddEnemy(NPCController enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void RemoveEnemy(NPCController enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    public NPCController GetRandomEnemy()
    {
        if (enemies.Count == 0) return null;
        NPCController randomNPC = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        Debug.Log($"the chosen NPC: {randomNPC.gameObject.name}");
        return randomNPC;
    }
}
