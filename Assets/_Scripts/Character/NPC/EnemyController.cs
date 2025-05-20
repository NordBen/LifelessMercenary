using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace LM.NPC
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] public List<GameObject> patrolPoints;
        [SerializeField] public BehaviorGraphAgent behavior;
        
        void Start()
        {
            behavior.BlackboardReference.SetVariableValue("PatrolPoints", patrolPoints);
        }
        
        void Update() {}
    }
}