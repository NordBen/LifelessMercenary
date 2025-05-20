using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "Line of Sight Check", story: "Check [Target] with [Detector]", category: "Conditions", id: "b2f17c500107ef3d65ed38e138655d91")]
public partial class AIC_LineOfSightCheck : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<LineOfSightDetection> Detector;

    public override bool IsTrue()
    {
        return Detector.Value.PerformDetection(Target.Value) != null;
    }
}
