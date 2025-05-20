using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Try Detect Target", story: "Try Detect [Target]", category: "Action", id: "c500107ef3d65ed38e138655d")]
public partial class AIA_RangeDetector : Action
{
    [SerializeReference] public BlackboardVariable<InRangeDetection> Detector;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    protected override Status OnStart()
    {
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        Target.Value = Detector.Value.UpdateDetector();
        return Target.Value == null ? Status.Failure : Status.Success;
    }
    
    protected override void OnEnd() { }
}