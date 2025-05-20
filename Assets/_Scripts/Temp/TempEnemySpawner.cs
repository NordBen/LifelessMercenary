using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempEnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject MobToSpawn;
    [SerializeField] private List<GameObject> Waypoints;
    [SerializeField] private GameObject PlayerTransform;
    [SerializeField] private float spawnTime = 2f;

    private float elapsedTime = 0f;
    private bool spawn = false;

    private void Start()
    {
        elapsedTime = 0f;
    }

    private void Update()
    {
        if (elapsedTime >= spawnTime)
        {
            elapsedTime = 0f;
            StartCoroutine(SpawnMob());
        }
        elapsedTime += Time.deltaTime;
    }

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