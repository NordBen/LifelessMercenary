using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject MobToSpawn;
    [SerializeField] private List<GameObject> Waypoints;
    [SerializeField] private GameObject PlayerTransform;
    [SerializeField] private float spawnTime = 2f;
    [SerializeField] private int maxSpawns;

    private float elapsedTime = 0f;
    private bool spawn = false;
    private int spawned = 0;

    private void Start()
    {
        elapsedTime = 0f;
        spawned = 0;
    }

    private void Update()
    {
        if (CanSpawn())
        {
            if (elapsedTime >= spawnTime)
            {
                elapsedTime = 0f;
                StartCoroutine(SpawnMob());
                spawned++;
            }
            elapsedTime += Time.deltaTime;
        }
    }
    
    private bool CanSpawn() => maxSpawns > 0 ? spawned < maxSpawns : true;

    private IEnumerator SpawnMob()
    {
        spawn = true;
        if (spawn)
        {
            GameObject mob = Instantiate(MobToSpawn, transform.position, Quaternion.identity);
            mob.GetComponent<LM.NPC.EnemyController>().patrolPoints = Waypoints;
            mob.GetComponent<LM.NPC.EnemyController>().behavior.BlackboardReference.SetVariableValue("Target", PlayerTransform);
            yield return null;
        }
        spawn = false;
    }
}