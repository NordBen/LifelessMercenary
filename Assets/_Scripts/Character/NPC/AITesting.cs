using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerDetector))]
public class AITesting : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerDetector playerDetector;

    [SerializeField] private float wanderRadius = 10f;

    [SerializeField] float timeBetweenAttacks = 1f;
    private CountdownTimer attackTimer;
    
    private AITestStateMachine stateMachine;

    private void OnValidate()
    {
        
    }

    private void Start()
    {
        attackTimer = new CountdownTimer(timeBetweenAttacks);
        
        stateMachine = new AITestStateMachine();
        var wanderState = new EnemyWanderState(this, animator, agent, wanderRadius);
        var chaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
        var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
        
        At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
        At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
        At(chaseState, attackState, new FuncPredicate(() => playerDetector.InAttackingDistance()));
        At(attackState, chaseState, new FuncPredicate(() => !playerDetector.InAttackingDistance()));
        
        stateMachine.SetState(wanderState);
    }

    private void Update()
    {
        stateMachine.Update();
        attackTimer.Tick(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

    public void Attack()
    {
        if (attackTimer.IsRunning) return;
        
        attackTimer.Start();
        Debug.Log("Attack");
    }
}

public abstract class EnemyBaseState : IState
{
    protected readonly AITesting enemy;
    protected readonly Animator animator;
    
    // animationHashes
    protected static readonly int IdleHash = Animator.StringToHash("isIdle");
    protected static readonly int WalkHash = Animator.StringToHash("isWalking");
    protected static readonly int ChaseHash = Animator.StringToHash("isChasing");
    protected static readonly int AttackHash = Animator.StringToHash("isAttacking");
    protected static readonly int DeathHash = Animator.StringToHash("tDead");
    protected static readonly int HitHash = Animator.StringToHash("tHit");
    protected static readonly int SkillHash = Animator.StringToHash("isSkill");
    
    protected const float crossFadeDuration = 0.1f;

    protected EnemyBaseState(AITesting enemy, Animator animator)
    {
        this.enemy = enemy;
        this.animator = animator;
    }

    public virtual void OnEnter()
    {
    }

    public virtual void Update()
    {
    }

    public virtual void FixedUpdate()
    {
    }

    public virtual void OnExit()
    {
        
    }
}

public interface IState
{
    void OnEnter();
    void Update();
    void FixedUpdate();
    void OnExit();
}

public interface IPredicate
{
    bool Evaluate();
}

public class FuncPredicate : IPredicate
{
    private readonly Func<bool> func;

    public FuncPredicate(Func<bool> func)
    {
        this.func = func;
    }
    
    public bool Evaluate() => func.Invoke();
}

public interface ITransition
{
    IState To { get; }
    IPredicate Condition { get; }
}

public class Transition : ITransition
{
    public IState To { get; }
    public IPredicate Condition { get; }

    public Transition(IState to, IPredicate condition)
    {
        To = to;
        Condition = condition;
    }
}

public class AITestStateMachine
{
    private StateNode current;
    private Dictionary<Type, StateNode> nodes = new();
    HashSet<ITransition> anyTransitions = new();

    public void Update()
    {
        current.State?.Update();
    }

    public void FixedUpdate()
    {
        current.State?.FixedUpdate();
    }

    public void SetState(IState state)
    {
        current = nodes[state.GetType()];
        current.State?.OnEnter();
    }

    public void ChangeState(IState state)
    {
        if (state == current.State) return;

        var previousState = current.State;
        var nextState = nodes[state.GetType()].State;

        previousState?.OnExit();
        nextState?.OnEnter();
        current = nodes[state.GetType()];
    }

    private ITransition GetTransition()
    {
        foreach (var transition in anyTransitions)
        {
            if (transition.Condition.Evaluate())
                return transition;
        }

        foreach (var transition in current.Transitions)
        {
            if (transition.Condition.Evaluate())
                return transition;
        }
        
        return null;
    }
    
    public void AddTransition(IState from, IState to, IPredicate condition)
    {
        GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
    }

    public void AddAnyTransition(IState to, IPredicate condition)
    {
        anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
    }

    StateNode GetOrAddNode(IState state)
    {
        var node = nodes.GetValueOrDefault(state.GetType());

        if (node == null)
        {
            node = new StateNode(state);
            nodes.Add(state.GetType(), node);
        }
        
        return node;
    }

    class StateNode
    {
        public IState State { get; }
        public HashSet<ITransition> Transitions { get; }

        public StateNode(IState state)
        {
            State = state;
            Transitions = new HashSet<ITransition>();
        }

        public void AddTransition(IState to, IPredicate condition)
        {
            Transitions.Add(new Transition(to, condition));
        }
    }
}

public class EnemyWanderState : EnemyBaseState
{
    readonly NavMeshAgent agent;
    readonly Vector3 startPoint;
    readonly float wanderRadius;
    
    public EnemyWanderState(AITesting enemy, Animator animator, NavMeshAgent agent, float radius) : base(enemy, animator)
    {
        this.agent = agent;
        this.startPoint = enemy.transform.position;
        this.wanderRadius = radius;
    }

    public override void OnEnter()
    {
        Debug.Log("Wandering");
        //animator.CrossFade(WalkHash, crossFadeDuration);
    }

    override public void Update()
    {
        if (HasReachedDestination())
        {
            var randomDirection = UnityEngine.Random.insideUnitSphere * wanderRadius;
            randomDirection += startPoint;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
            var finalDestination = hit.position;
            agent.SetDestination(finalDestination);
        }
    }

    public override void OnExit()
    {
        Debug.Log("Wander Exit");
    }

    private bool HasReachedDestination()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }
}

public class EnemyChaseState : EnemyBaseState
{
    private readonly NavMeshAgent agent;
    private readonly Transform target;
    
    public EnemyChaseState(AITesting enemy, Animator animator, NavMeshAgent agent, Transform target) : base(enemy, animator)
    {
        this.agent = agent;
        this.target = target;
    }

    public override void OnEnter()
    {
        Debug.Log("Chasing");
        //animator.CrossFade(ChaseHash, crossFadeDuration);
    }

    override public void Update()
    {
        agent.SetDestination(target.position);
    }

    public override void OnExit()
    {
        Debug.Log("Chase Exit");
    }
}

public class EnemyAttackState : EnemyBaseState
{
    private readonly NavMeshAgent agent;
    private readonly Transform player;
    
    public EnemyAttackState(AITesting enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator)
    {
        this.agent = agent;
        this.player = player;
    }

    override public void OnEnter()
    {
        Debug.Log("Attacking");
        //animator.CrossFade(AttackHash, crossFadeDuration);
    }

    public override void Update()
    {
        agent.SetDestination(player.position);
        enemy.Attack();
    }

    override public void OnExit()
    {
        Debug.Log("Attacking ended");
    }
}