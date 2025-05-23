using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using LM.AbilitySystem;
using LM.NPC;

namespace LM
{
    public class GameManager : MonoBehaviour, IDataPersistance
    {
        public static GameManager instance;
        public Player player;

        public List<LM.NPC.EnemyController> enemies;

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
                    LoadButtonScene();
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

        private void LoadButtonScene()
        {
            SceneManager.LoadScene("ButtonScene");
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
            GameObject.Find("SceneDude").GetComponent<TempCutscenePosition>().PlayDirector();

            ModifyDeathEnergy(-1);

            Debug.Log(
                $"dur to die: {(float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration}");
            Invoke("Death",
                (float)GameObject.Find("SceneDude").GetComponent<PlayableDirector>().playableAsset.duration);
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

        public void StartGame()
        {

        }

        private IEnumerator FullHeali()
        {
            yield return null;
            yield return null;
            DataPersistenceManager.instance.LoadGame();
            var GAC = player.GetComponent<GameplayAttributeComponent>();
            if (GAC != null)
            {
                Debug.Log("full heal");
                GAC.ApplyEffect(GAC._fullHealEffect, false);
            }

            yield break;
        }

        public void AddEnemy(LM.NPC.EnemyController enemy)
        {
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
            }
        }

        public void RemoveEnemy(LM.NPC.EnemyController enemy)
        {
            if (enemies.Contains(enemy))
            {
                enemies.Remove(enemy);
            }
        }

        public LM.NPC.EnemyController GetRandomEnemy()
        {
            if (enemies.Count == 0) return null;
            LM.NPC.EnemyController randomNPC = enemies[UnityEngine.Random.Range(0, enemies.Count)];
            Debug.Log($"the chosen NPC: {randomNPC.gameObject.name}");
            return randomNPC;
        }

        public void SaveData(SaveGameData data)
        {
            data.daysSurvived = daysSurvived;
            data.deathEnergy = currentDeathEnergy;
        }

        public void LoadData(SaveGameData data)
        {
            daysSurvived = data.daysSurvived;
            currentDeathEnergy = data.deathEnergy;
        }
    }
}