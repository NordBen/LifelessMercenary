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

    public List<NPCControllerTemp> enemies = new();

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

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
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
        player = GameObject.Find("Player").GetComponent<Player>();
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
        //GameObject.Find("SceneDude").GetComponent<PlayableDirector>().Play();
        ModifyDeathEnergy(-1);

        //Debug.Log($"dur to die: {(float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration}");
        Invoke("Death", 1.5f);//(float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration);
    }

    public void Death()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    public void ResetLoop()
    {
        SceneManager.LoadScene(1);//"TheLevelScene");
    }

    public NPCControllerTemp GetRandomEnemy()
    {
        if (enemies.Count == 0) return null;
        NPCControllerTemp randomNPC = enemies[UnityEngine.Random.Range(0, enemies.Count)];
        Debug.Log($"the chosen NPC: {randomNPC.gameObject.name}");
        return randomNPC;
    }

    public void AddEnemy(NPCControllerTemp enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    public void RemoveEnemy(NPCControllerTemp enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }
}
