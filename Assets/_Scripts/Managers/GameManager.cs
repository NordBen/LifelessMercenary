using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using LM;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public Player player;

    public List<NPCController> enemies;

    public GameObject sceneDude;
    
    public event Action<int> OnDeathEnergyChanged;
    [SerializeField] private int deathEnergyMax = 5;
    [SerializeField] private int currentDeathEnergy = 5;

    public event Action<int> OnDaySurvived;
    [SerializeField] private int daysSurvived = 1;

    public Transform DayTwo;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            ///Destroy(gameObject);
        }
        //if (player == null && GameObject.Find("Player"))
        //    player = GameObject.Find("Player").GetComponent<Player>();
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
        
        if (Input.GetKeyDown(KeyCode.N))
        {
            Debug.Log("new game");
            DataPersistenceManager.instance.NewGame();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            DataPersistenceManager.instance.SaveGame();
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
        //player.GetAnimator().SetTrigger("tDead");
        //GameObject.Find("SceneDude").GetComponent<PlayableDirector>().Play();
        GameObject.Find("SceneDude").GetComponent<TempCutscenePosition>().PlayDirector();
        
        ModifyDeathEnergy(-1);

        Debug.Log($"dur to die: {(float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration}");
        Invoke("Death", (float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration);
    }
    
    public void KillPlayerRagdoll()
    {
        //player.GetPlayerController.ToggleRagdoll();
        
        ModifyDeathEnergy(-1);
        
        Invoke("Death", 1f);
    }

    public void Death()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    public void DeathRealm()
    {
        SceneManager.LoadScene("DeathScene");
    }

    public void ResetLoop()
    {
        DataPersistenceManager.instance.SaveGame();
        StartCoroutine("FullHeali", 1f);
        SceneManager.LoadScene("TheLevelScene");
    }

    private IEnumerator FullHeali()
    {
        DataPersistenceManager.instance.LoadGame();
        yield return null;
        FullHealPlayer();
        yield return null;
    }
    
    private void FullHealPlayer()
    {
        var gameplayAttributeComponent = player.GetComponent<GameplayAttributeComponent>();
        GameplayEffectApplication healthApplication = new GameplayEffectApplication(
            gameplayAttributeComponent.GetAttribute("Health"),
            EModifierOperationType.Override,
            new AttributeBasedValueStrategy
            {
                sourceAttribute = gameplayAttributeComponent.GetAttribute("MaxHealth"),
                _coefficient = 1
            });
        GameplayEffectApplication StaminaApplication = new GameplayEffectApplication(
            gameplayAttributeComponent.GetAttribute("Stamina"),
            EModifierOperationType.Override,
            new AttributeBasedValueStrategy
            {
                sourceAttribute = gameplayAttributeComponent.GetAttribute("MaxStamina"),
                _coefficient = 1
            });
        List<GameplayEffectApplication> applications = new List<GameplayEffectApplication>();
        applications.Add(healthApplication);
        applications.Add(StaminaApplication);
        GameplayEffect fixHealthNStaminaEffect = GameplayEffectFactory.CreateEffect(
            "FullHeal",
            EEffectDurationType.Duration,
            1,
            applications
        );
        gameplayAttributeComponent.ApplyEffect(fixHealthNStaminaEffect, true);
        Debug.Log(
            $"Health after reset: {gameplayAttributeComponent.GetAttribute("Health").CurrentValue} / {gameplayAttributeComponent.GetAttribute("MaxHealth").CurrentValue}");
        Debug.Log(
            $"Stamina after reset: {gameplayAttributeComponent.GetAttribute("Stamina").CurrentValue} / {gameplayAttributeComponent.GetAttribute("MaxStamina").CurrentValue}");
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
