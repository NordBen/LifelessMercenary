using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VectorMath
{
    public static float GetAngle(Vector3 vector1, Vector3 vector2, Vector3 planeNormal)
    {
        var angle = Vector3.Angle(vector1, vector2);
        var sign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(vector1, vector2)));
        return angle * sign;
    }

    public static float GetDotProduct(Vector3 vector, Vector3 direction) => Vector3.Dot(vector, direction.normalized);

    public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return vector - direction * Vector3.Dot(vector, direction);
    }

    public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction)
    {
        direction.Normalize();
        return direction * Vector3.Dot(vector, direction);
    }

    public static Vector3 RotateVectorOntoPlane(Vector3 vector, Vector3 planeNormal, Vector3 upDirection)
    {
        var rotation = Quaternion.FromToRotation(upDirection, planeNormal);
        vector = rotation * vector;
        return vector;
    }

    public static Vector3 ProjectPointOntoLine(Vector3 lineStartPos, Vector3 lineDirection, Vector3 point)
    {
        var projectLine = point - lineStartPos;
        var dotProduct = Vector3.Dot(projectLine, lineDirection);
        return lineStartPos + lineDirection * dotProduct;
    }

    public static Vector3 IncrementVectorTowardTargetVector(Vector3 currentVector, float speed, float deltaTime, Vector3 targetVector)
    {
        return Vector3.MoveTowards(currentVector, targetVector, speed * deltaTime);
    }
}

public class ExtStateMachine
{
    StateNode currentNode;
    readonly Dictionary<Type, StateNode> nodes = new();
    readonly HashSet<TransitionS2> anyTransitions = new();

    public IStateS2 CurrentState => currentNode.State;

    public void Update()
    {
        var transition = GetTransition();

        if (transition != null)
        {
            ChangeState(transition.To);
            foreach (var node in nodes.Values)
            {
                ResetActionPredicateFlags(node.Transitions);
            }
            ResetActionPredicateFlags(anyTransitions);
        }

        currentNode.State?.Update();
    }

    static void ResetActionPredicateFlags(IEnumerable<TransitionS2> transitions)
    {
        foreach (var transition in transitions.OfType<TransitionS2<ActionPredicateS2>>())
        {
            transition.condition.flag = false;
        }
    }

    public void FixedUpdate()
    {
        currentNode.State?.FixedUpdate();
    }

    public void SetState(IStateS2 state)
    {
        currentNode = nodes[state.GetType()];
        currentNode.State?.OnEnter();
    }

    void ChangeState(IStateS2 state)
    {
        if (state == currentNode.State)
            return;

        var previousState = currentNode.State;
        var nextState = nodes[state.GetType()].State;

        previousState?.OnExit();
        nextState.OnEnter();
        currentNode = nodes[state.GetType()];
    }

    public void AddTransition<T>(IStateS2 from, IStateS2 to, T condition)
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
    }

    public void AddAnyTransition<T>(IStateS2 to, T condition)
    {
        anyTransitions.Add(new TransitionS2<T>(GetOrAddNode(to).State, condition));
    }

    TransitionS2 GetTransition()
    {
        foreach (var transition in anyTransitions)
            if (transition.Evaluate())
                return transition;

        foreach (var transition in currentNode.Transitions)
        {
            if (transition.Evaluate())
                return transition;
        }

        return null;
    }

    StateNode GetOrAddNode(IStateS2 state)
    {
        var node = nodes.GetValueOrDefault(state.GetType());
        if (node == null)
        {
            node = new StateNode(state);
            nodes[state.GetType()] = node;
        }

        return node;
    }

    class StateNode
    {
        public IStateS2 State { get; }
        public HashSet<TransitionS2> Transitions { get; }

        public StateNode(IStateS2 state)
        {
            State = state;
            Transitions = new HashSet<TransitionS2>();
        }

        public void AddTransition<T>(IStateS2 to, T predicate)
        {
            Transitions.Add(new TransitionS2<T>(to, predicate));
        }
    }
}

public interface ITransitionS2
{
    IStateS2 To { get; }
    IPredicateS2 Condition { get; }
}

public interface IPredicateS2
{
    bool Evaluate();
}

public class And : IPredicateS2
{
    [SerializeField] List<IPredicateS2> rules = new List<IPredicateS2>();
    public bool Evaluate() => rules.All(r => r.Evaluate());
}

public class Or : IPredicateS2
{
    [SerializeField] List<IPredicateS2> rules = new List<IPredicateS2>();
    public bool Evaluate() => rules.Any(r => r.Evaluate());
}

public class Not : IPredicateS2
{
    [SerializeField] IPredicateS2 rule;
    public bool Evaluate() => !rule.Evaluate();
}

public abstract class TransitionS2
{
    public IStateS2 To { get; protected set; }
    public abstract bool Evaluate();
}

public class TransitionS2<T> : TransitionS2
{
    public readonly T condition;

    public TransitionS2(IStateS2 to, T condition)
    {
        To = to;
        this.condition = condition;
    }

    public override bool Evaluate()
    {
        var result = (condition as Func<bool>)?.Invoke();
        if (result.HasValue)
        {
            return result.Value;
        }
        result = (condition as ActionPredicateS2)?.Evaluate();
        if (result.HasValue)
        {
            return result.Value;
        }
        result = (condition as IPredicateS2)?.Evaluate();
        if (result.HasValue)
        {
            return result.Value;
        }
        return false;
    }
}

public class FuncPredicateS2 : IPredicateS2
{
    readonly Func<bool> func;

    public FuncPredicateS2(Func<bool> func)
    {
        this.func = func;
    }

    public bool Evaluate() => func.Invoke();
}

public class ActionPredicateS2 : IPredicateS2
{
    public bool flag;

    public ActionPredicateS2(ref Action eventReaction) => eventReaction += () => { flag = true; };

    public bool Evaluate()
    {
        bool result = flag;
        flag = false;
        return result;
    }
}