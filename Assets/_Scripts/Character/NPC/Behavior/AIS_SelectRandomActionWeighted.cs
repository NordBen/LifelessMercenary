using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Select Random Action With Weights", story: "Randomly selects one child node to execute based on assigned weights", category: "Flow", id: "3d63e94287eab254fa06b1f29eb27397")]
public partial class AIS_SelectRandomActionWeighted : Composite
{
    [SerializeReference, CreateProperty]
    public BlackboardVariable<bool> NormalizeWeights3 = new BlackboardVariable<bool>(true);

    [Serializable, GeneratePropertyBag]
    public class ChildNodeWeight
    {
        [CreateProperty] public int NodeIndex;
        [CreateProperty] public float Weight = 1.0f;
    }
    
    [CreateProperty] public List<ChildNodeWeight> nodeWeights = new List<ChildNodeWeight>();
    [SerializeReference, CreateProperty] public BlackboardVariable<bool> NormalizeWeights = new BlackboardVariable<bool>(true);
    [SerializeField, CreateProperty] public bool NormalizeWeights2 = true;

    private int _selectedChildNodeAction = -1;
    private bool _hasSelectedChildNode = false;
    private Node _selectedChildNode = null;

    protected override Status OnStart()
    {
        _hasSelectedChildNode = false;
        _selectedChildNodeAction = -1;
        _selectedChildNode = null;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!_hasSelectedChildNode)
        {
            _selectedChildNodeAction = SelectRandomChildByWeight();
            if (_selectedChildNodeAction >= 0 && _selectedChildNodeAction < Children.Count)
            {
                _selectedChildNode = Children[_selectedChildNodeAction];
                _hasSelectedChildNode = true;
            }
            else
            {
                return Status.Failure;
            }
            /*
            Status childStartStatus = Children[(_selectedChildNodeAction)].Start();
            if (childStartStatus != Status.Running)
                return childStartStatus;*/
        }

        if (_selectedChildNode != null)
        {
            return _selectedChildNode.CurrentStatus;
        }
        return Status.Failure;
    }

    protected override void OnEnd()
    {
        _selectedChildNode = null;
    }

    private int SelectRandomChildByWeight()
    {
        if (Children.Count == 0)
            return -1;

        if (nodeWeights.Count == 0)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                nodeWeights.Add(new ChildNodeWeight() { NodeIndex = i, Weight = 1.0f});
            }
        }

        for (int i = 0; i < Children.Count; i++)
        {
            if (!nodeWeights.Any(c => c.NodeIndex == i))
            {
                nodeWeights.Add(new ChildNodeWeight { NodeIndex = i, Weight = 1.0f });
            }
        }
        
        nodeWeights.RemoveAll(w => w.NodeIndex >= Children.Count);
        
        List<float> workingWeights = nodeWeights.OrderBy(w => w.NodeIndex).Select(w => w.Weight).ToList();

        if (NormalizeWeights)
        {
            float sum = workingWeights.Sum();
            if (sum > 0)
            {
                for (int i = 0; i < workingWeights.Count; i++)
                {
                    workingWeights[i] = workingWeights[i] / sum;
                }
            }
            else
            {
                float equalWeight = 1.0f / workingWeights.Count;
                for (int i = 0; i < workingWeights.Count; i++)
                {
                    workingWeights[i] = equalWeight;
                }
            }
        }
        
        float totalWeight = workingWeights.Sum();
        
        if (totalWeight <= 0)
            UnityEngine.Random.Range(0, Children.Count);
        
        float randomAction = UnityEngine.Random.Range(0, totalWeight);

        float weightSum = 0;
        for (int i = 0; i < workingWeights.Count; i++)
        {
            weightSum += workingWeights[i];
            if (randomAction <= weightSum)
                return i;
        }
        return Children.Count - 1;
    }
}