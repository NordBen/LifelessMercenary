using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

namespace LM.NPC
{
    public class EnemyController : BaseCharacter
    {
        [SerializeField] public List<GameObject> patrolPoints;
        [SerializeField] public BehaviorGraphAgent behavior;
        
        void Start()
        {
            base.Start();
            SetBlackboardVariables();
        }

        void Update()
        {
            base.Update();
        }

        public override void Die()
        {
            base.Die();
            _animator.SetBool(IsDeadHash, true);
            GameManager.instance.RemoveEnemy(this);
            Invoke("Destroy()", 8f);
        }
        
        private void SetBlackboardVariables()
        {
            behavior.BlackboardReference.SetVariableValue("PatrolPoints", patrolPoints);
            behavior.BlackboardReference.SetVariableValue("WalkSpeed", this.walkSpeed);
            behavior.BlackboardReference.SetVariableValue("RunSpeed", this.runSpeed);
        }
    }
}