using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

namespace LM.NPC
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(name: "Attack", story: "Attack [Target]", category: "Action",
        id: "35d0e401ff79980349019b6c8f88895f")]
    public partial class AIA_PerformAttack : Action
    {
        [SerializeReference] public BlackboardVariable<string> Trigger;
        [SerializeReference] public BlackboardVariable<Animator> Animator;
        [SerializeReference] public BlackboardVariable<bool> TriggerState;

        protected override Status OnStart()
        {
            if (Animator.Value == null)
            {
                LogFailure("No Animator set.");
                return Status.Failure;
            }
            if (TriggerState.Value) { Animator.Value.SetTrigger(Trigger.Value); } 
            else { Animator.Value.ResetTrigger(Trigger.Value); }
            
            return Status.Success;
        }

        protected override Status OnUpdate()
        {
            return Status.Success;
        }

        protected override void OnEnd() {}
    }
}