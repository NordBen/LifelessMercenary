using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
/*
#region AIBehaviorController
public class AIBehaviorController : MonoBehaviour
{
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float lowHealthThreshold = 0.15f; // 15% of max health
    private float attackDistance;
    
    private NavMeshAgent agent;
    private Animator animator;
    private AIHealth health;
    private Transform player;
    private AIState currentState;

    public enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Retreat,
        Heal,
        CallReinforcements,
        Block,
        Parry,
        Death
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        health = GetComponent<AIHealth>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Start in patrol state
        ChangeState(AIState.Patrol);
    }

    private void Update()
    {
        if (IsPlayerDetected() && currentState != AIState.Chase)
        {
            ChangeState(AIState.Chase);
        }

        UpdateCurrentState();
    }

    private bool IsPlayerDetected()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle <= fieldOfView * 0.5f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= detectionRange)
            {
                // Check if there are obstacles between AI and player
                if (!Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer,
                        obstacleLayer))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void UpdateCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                UpdatePatrolState();
                break;
            case AIState.Chase:
                UpdateChaseState();
                break;
            case AIState.Attack:
                UpdateAttackState();
                break;
            // Add other states...
        }

        // Check for low health condition
        if (health.CurrentHealth <= health.MaxHealth * lowHealthThreshold)
        {
            HandleLowHealth();
        }
    }
    
    private bool IsObstructed()
    {
        if (player == null) return true;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
    
        return Physics.Raycast(transform.position, directionToPlayer, 
            distanceToPlayer, obstacleLayer);
    }

    public void ChangeState(AIState newState)
    {
        currentState = newState;
        // Reset appropriate variables for new state
    }

    private void UpdatePatrolState()
    {
        PatrolSystem patrolSystem = GetComponent<PatrolSystem>();
        if (patrolSystem != null)
        {
            patrolSystem.UpdatePatrol();
        }
    }

    private void UpdateChaseState()
    {
        if (player == null) return;

        PerceptionSystem perception = GetComponent<PerceptionSystem>();
        CombatStrategy combat = GetComponent<CombatStrategy>();

        if (perception != null && combat != null)
        {
            if (perception.CanSeeTarget() || perception.CanHearTarget())
            {
                // Get ideal combat position
                Vector3 targetPosition = combat.GetIdealPosition();

                // Move to position
                agent.SetDestination(targetPosition);

                // If we're in attack range and have a clear shot, switch to attack state
                float distanceToPlayer = Vector3.Distance(transform.position, player.position);
                if (distanceToPlayer <= attackDistance && !IsObstructed())
                {
                    ChangeState(AIState.Attack);
                }
            }
            else if (perception.HasRecentMemory())
            {
                // Move to last known position
                agent.SetDestination(perception.LastKnownPosition);
            }
            else
            {
                // Lost the player, return to patrol
                ChangeState(AIState.Patrol);
            }
        }
    }

    private void UpdateAttackState()
    {
        if (player == null) return;

        CombatStrategy combat = GetComponent<CombatStrategy>();
        if (combat != null)
        {
            if (combat.ShouldReposition())
            {
                agent.SetDestination(combat.GetIdealPosition());
                return;
            }

            // Decide and perform action
            string nextAction = combat.DecideNextAction();
            switch (nextAction)
            {
                case "Attack":
                    GetComponent<AIAttackSystem>()?.PerformAttack();
                    break;
                case "Block":
                    GetComponent<CombatSystem>()?.StartBlocking();
                    break;
                case "Heal":
                    GetComponent<AIHealth>()?.UseHealthPotion();
                    break;
                // Add other actions as needed
            }
        }
    }

    private void HandleLowHealth()
    {
        AIHealth health = GetComponent<AIHealth>();
        if (health != null && health.CanUseHealthPotion())
        {
            // Increase chance of healing when low on health
            GetComponent<CombatStrategy>()?.AdjustWeight("Heal", 2.0f);

            if (Random.value < 0.7f) // 70% chance to retreat and heal
            {
                ChangeState(AIState.Retreat);
            }
        }
    }
}
#endregion

#region AIHealth
public class AIHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float healthPotionHealAmount = 30f;
    [SerializeField] private float healthPotionCooldown = 10f;
    [SerializeField] private GameObject healthBarPrefab;
    
    public float CurrentHealth { get; private set; }
    public float MaxHealth => maxHealth;
    
    private float lastPotionTime;
    private HealthBar healthBar;
    private AIBehaviorController aiController;

    private void Start()
    {
        CurrentHealth = maxHealth;
        aiController = GetComponent<AIBehaviorController>();
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (healthBarPrefab != null)
        {
            var barObject = Instantiate(healthBarPrefab, transform);
            healthBar = barObject.GetComponent<HealthBar>();
            UpdateHealthBar();
        }
    }

    public void TakeDamage(float damage, Vector3 direction, float knockbackForce)
    {
        if (aiController.IsBlocking)
        {
            damage *= 0.2f; // Reduce damage when blocking
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        UpdateHealthBar();

        // Apply knockback
        if (knockbackForce > 0)
        {
            ApplyKnockback(direction, knockbackForce);
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public bool CanUseHealthPotion()
    {
        return Time.time - lastPotionTime >= healthPotionCooldown && 
               CurrentHealth < maxHealth;
    }

    public void UseHealthPotion()
    {
        if (!CanUseHealthPotion()) return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + healthPotionHealAmount);
        lastPotionTime = Time.time;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(CurrentHealth / maxHealth);
        }
    }

    private void ApplyKnockback(Vector3 direction, float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    private void Die()
    {
        aiController.ChangeState(AIBehaviorController.AIState.Death);
        // Additional death logic here
    }
}
#endregion

#region PatrolSystem
public class PatrolSystem : MonoBehaviour
{
    [System.Serializable]
    public class PatrolPoint
    {
        public Vector3 position;
        public float waitTime = 2f;
    }

    [SerializeField] private PatrolPoint[] patrolPoints;
    [SerializeField] private bool randomizePoints = false;
    [SerializeField] private float patrolSpeed = 3f;
    [SerializeField] private float arrivalDistance = 0.5f;

    private NavMeshAgent agent;
    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (randomizePoints)
        {
            ShufflePatrolPoints();
        }
    }

    public void UpdatePatrol()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolPoints[currentPointIndex].waitTime)
            {
                isWaiting = false;
                MoveToNextPoint();
            }
            return;
        }

        if (HasReachedCurrentPoint())
        {
            StartWaiting();
        }
    }

    private bool HasReachedCurrentPoint()
    {
        if (patrolPoints.Length == 0) return false;
        
        return Vector3.Distance(transform.position, 
            patrolPoints[currentPointIndex].position) <= arrivalDistance;
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = 0f;
        agent.velocity = Vector3.zero;
    }

    private void MoveToNextPoint()
    {
        currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
    }

    private void ShufflePatrolPoints()
    {
        for (int i = patrolPoints.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            var temp = patrolPoints[i];
            patrolPoints[i] = patrolPoints[randomIndex];
            patrolPoints[randomIndex] = temp;
        }
    }

    public void AddPatrolPoint(Vector3 position, float waitTime = 2f)
    {
        Array.Resize(ref patrolPoints, patrolPoints.Length + 1);
        patrolPoints[patrolPoints.Length - 1] = new PatrolPoint 
        { 
            position = position, 
            waitTime = waitTime 
        };
    }
}
#endregion

#region AIAttackSystem
public class AIAttackSystem : MonoBehaviour
{
    [System.Serializable]
    public class AttackDefinition
    {
        public string animationName;
        public float damage;
        public float range;
        public float cooldown;
        public float weight = 1f; // For random selection
    }

    [SerializeField] private AttackDefinition[] attacks;
    [SerializeField] private float attackDistance = 2f;
    
    private Animator animator;
    private bool isAttacking;
    private float lastAttackTime;

    public void PerformAttack()
    {
        if (isAttacking || Time.time - lastAttackTime < GetCurrentAttack().cooldown)
            return;

        AttackDefinition attack = SelectRandomAttack();
        animator.Play(attack.animationName);
        isAttacking = true;
        lastAttackTime = Time.time;
    }

    private AttackDefinition SelectRandomAttack()
    {
        float totalWeight = 0f;
        foreach (var attack in attacks)
        {
            totalWeight += attack.weight;
        }

        float random = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var attack in attacks)
        {
            current += attack.weight;
            if (random <= current)
                return attack;
        }

        return attacks[0];
    }

    // Animation event callback
    public void OnAttackComplete()
    {
        isAttacking = false;
    }
}
#endregion

#region CombatSystem
public class CombatSystem : MonoBehaviour
{
    [SerializeField] private float parryWindow = 0.2f;
    [SerializeField] private float blockStaminaCost = 10f;
    [SerializeField] private float parryStaminaCost = 15f;
    
    private Animator animator;
    private bool isBlocking;
    private bool canParry;
    private float parryTimer;

    public void StartBlocking()
    {
        if (HasEnoughStamina(blockStaminaCost))
        {
            isBlocking = true;
            animator.SetBool("IsBlocking", true);
        }
    }

    public void StopBlocking()
    {
        isBlocking = false;
        animator.SetBool("IsBlocking", false);
    }

    public void AttemptParry()
    {
        if (HasEnoughStamina(parryStaminaCost))
        {
            canParry = true;
            parryTimer = parryWindow;
            animator.SetTrigger("ParryAttempt");
        }
    }

    public void HandleIncomingAttack(Attack attack)
    {
        if (canParry && parryTimer > 0)
        {
            PerformParry(attack.attacker);
        }
        else if (isBlocking)
        {
            BlockAttack(attack);
        }
        else
        {
            TakeDamage(attack.damage);
        }
    }

    private void PerformParry(Transform attacker)
    {
        // Play parry animation
        animator.SetTrigger("ParrySuccess");
        
        // Stagger the attacker
        if (attacker.TryGetComponent<Animator>(out Animator attackerAnim))
        {
            attackerAnim.SetTrigger("Staggered");
        }
    }
}
#endregion

#region ExecutionSystem
public class ExecutionSystem : MonoBehaviour
{
    [SerializeField] private float executionDistance = 2f;
    [SerializeField] private string executionAnimationTrigger = "Execute";
    [SerializeField] private string victimAnimationTrigger = "BeExecuted";
    
    private Animator animator;

    public bool CanExecute(Transform target)
    {
        if (target.TryGetComponent<Health>(out Health targetHealth))
        {
            return targetHealth.CurrentHealth <= 0;
        }
        return false;
    }

    public void PerformExecution(Transform target)
    {
        if (!CanExecute(target)) return;

        // Position both characters correctly
        Vector3 executionPosition = target.position - transform.forward * executionDistance;
        transform.position = executionPosition;
        transform.LookAt(target);

        // Play animations
        animator.SetTrigger(executionAnimationTrigger);
        
        if (target.TryGetComponent<Animator>(out Animator targetAnimator))
        {
            targetAnimator.SetTrigger(victimAnimationTrigger);
        }
    }
}
#endregion

#region BossAI
public class BossAI : AIBehaviorController
{
    [SerializeField] private float parryChance = 0.3f;
    [SerializeField] private float blockChance = 0.5f;
    [SerializeField] private float phaseChangeHealthThreshold = 0.5f; // 50% health
    
    private int currentPhase = 1;

    protected override void Update()
    {
        base.Update();
        
        // Check for phase transition
        if (health.CurrentHealth <= health.MaxHealth * phaseChangeHealthThreshold && currentPhase == 1)
        {
            TransitionToPhase2();
        }
    }

    private void TransitionToPhase2()
    {
        currentPhase = 2;
        // Unlock new attacks, increase speed, etc.
        animator.SetTrigger("PhaseTransition");
    }

    public override void HandleIncomingAttack(Attack attack)
    {
        float random = Random.value;
        
        if (random <= parryChance)
        {
            AttemptParry();
        }
        else if (random <= blockChance)
        {
            StartBlocking();
        }
        else
        {
            base.HandleIncomingAttack(attack);
        }
    }
}
#endregion

#region AILearningSystem
public class AILearningSystem : MonoBehaviour
{
    [System.Serializable]
    public class PlayerAction
    {
        public string actionName;
        public float timestamp;
        public Vector3 position;
    }

    private List<PlayerAction> playerActionHistory = new List<PlayerAction>();
    private Dictionary<string, float> actionWeights = new Dictionary<string, float>();

    public void RecordPlayerAction(string actionName, Vector3 position)
    {
        playerActionHistory.Add(new PlayerAction
        {
            actionName = actionName,
            timestamp = Time.time,
            position = position
        });

        AnalyzePattern();
    }

    private void AnalyzePattern()
    {
        // Simple pattern recognition - adjust weights based on success/failure
        // of player actions against the AI
        // This is a basic example - you could make this much more sophisticated
        foreach (var action in playerActionHistory.TakeLast(10))
        {
            if (!actionWeights.ContainsKey(action.actionName))
                actionWeights[action.actionName] = 1f;

            // Adjust weights based on how successful the player's action was
            // You'll need to define your own success criteria
            if (WasActionSuccessful(action))
            {
                actionWeights[action.actionName] *= 1.1f; // Increase counter-action priority
            }
        }
    }
}
#endregion

#region PerceptionSystem
public class PerceptionSystem : MonoBehaviour
{
    [SerializeField] private float viewDistance = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float hearingRange = 15f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float memoryDuration = 3f;
    
    private Transform target;
    private Vector3 lastKnownPosition;
    private float lastSeenTime;
    private bool hasTarget;

    public bool HasTarget => hasTarget;
    public Vector3 LastKnownPosition => lastKnownPosition;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public bool CanSeeTarget()
    {
        if (target == null) return false;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);

        if (angle <= viewAngle * 0.5f)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            
            if (distanceToTarget <= viewDistance)
            {
                if (!Physics.Raycast(transform.position, directionToTarget, 
                    out RaycastHit hit, distanceToTarget, obstacleLayer))
                {
                    UpdateTargetMemory(target.position);
                    return true;
                }
            }
        }

        return false;
    }

    public bool CanHearTarget()
    {
        if (target == null) return false;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= hearingRange)
        {
            // Check if there's a direct path to the sound
            if (!Physics.Raycast(transform.position, 
                (target.position - transform.position).normalized, 
                out RaycastHit hit, distanceToTarget, obstacleLayer))
            {
                UpdateTargetMemory(target.position);
                return true;
            }
        }

        return false;
    }

    private void UpdateTargetMemory(Vector3 position)
    {
        lastKnownPosition = position;
        lastSeenTime = Time.time;
        hasTarget = true;
    }

    public bool HasRecentMemory()
    {
        if (Time.time - lastSeenTime > memoryDuration)
        {
            hasTarget = false;
            return false;
        }
        return hasTarget;
    }

    public void ResetMemory()
    {
        hasTarget = false;
        lastSeenTime = 0f;
    }
}
#endregion

#region CombatStrategy
public class CombatStrategy : MonoBehaviour
{
    [System.Serializable]
    public class ActionWeight
    {
        public string actionName;
        public float baseWeight = 1f;
        public float currentWeight;
        public float cooldown;
        private float lastUsedTime;

        public bool IsAvailable => Time.time - lastUsedTime >= cooldown;

        public void Use()
        {
            lastUsedTime = Time.time;
        }
    }

    [SerializeField] private ActionWeight[] actions;
    [SerializeField] private float preferredCombatDistance = 3f;
    [SerializeField] private float minAttackDistance = 1.5f;
    [SerializeField] private float maxAttackDistance = 4f;
    [SerializeField] private float repositionThreshold = 0.5f;

    private AIBehaviorController aiController;
    private Transform target;

    private void Start()
    {
        aiController = GetComponent<AIBehaviorController>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Initialize weights
        foreach (var action in actions)
        {
            action.currentWeight = action.baseWeight;
        }
    }

    public string DecideNextAction()
    {
        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // If we're too far or too close, prioritize repositioning
        if (distanceToTarget > maxAttackDistance || distanceToTarget < minAttackDistance)
        {
            return "Reposition";
        }

        // Calculate total weight of available actions
        float totalWeight = 0f;
        foreach (var action in actions)
        {
            if (action.IsAvailable)
            {
                totalWeight += action.currentWeight;
            }
        }

        // Random selection based on weights
        float random = Random.Range(0f, totalWeight);
        float currentTotal = 0f;

        foreach (var action in actions)
        {
            if (!action.IsAvailable) continue;

            currentTotal += action.currentWeight;
            if (random <= currentTotal)
            {
                action.Use();
                return action.actionName;
            }
        }

        return "Idle";
    }

    public Vector3 GetIdealPosition()
    {
        if (target == null) return transform.position;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        return target.position - directionToTarget * preferredCombatDistance;
    }

    public void AdjustWeight(string actionName, float multiplier)
    {
        var action = Array.Find(actions, a => a.actionName == actionName);
        if (action != null)
        {
            action.currentWeight *= multiplier;
        }
    }

    public bool ShouldReposition()
    {
        if (target == null) return false;

        float currentDistance = Vector3.Distance(transform.position, target.position);
        return Mathf.Abs(currentDistance - preferredCombatDistance) > repositionThreshold;
    }
}
#endregion

#region BaseStateMachine
public abstract class BaseStateMachine : MonoBehaviour
{
    protected Dictionary<Type, BaseState> availableStates;
    protected BaseState currentState;
    protected CharacterController characterController;

    protected virtual void Awake()
    {
        availableStates = new Dictionary<Type, BaseState>();
        characterController = GetComponent<CharacterController>();
    }

    public void SwitchState(BaseState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }

    protected virtual void Update()
    {
        currentState?.UpdateState();
    }

    protected void RegisterState(BaseState state)
    {
        availableStates.Add(state.GetType(), state);
    }

    public T GetState<T>() where T : BaseState
    {
        return availableStates.TryGetValue(typeof(T), out BaseState state) ? (T)state : null;
    }
}
#endregion

#region BaseState
public abstract class BaseState
{
    protected BaseStateMachine stateMachine;
    protected Animator animator;
    protected readonly int animationHash;

    protected BaseState(BaseStateMachine stateMachine, string animationName)
    {
        this.stateMachine = stateMachine;
        this.animator = stateMachine.GetComponent<Animator>();
        this.animationHash = Animator.StringToHash(animationName);
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
    
    protected virtual void PlayAnimation(string trigger, bool useHash = true)
    {
        if (useHash)
            animator.SetTrigger(animationHash);
        else
            animator.SetTrigger(trigger);
    }
}
#endregion

#region CombatSystem2
public class CombatSystem2 : MonoBehaviour
{
    [System.Serializable]
    public class AttackDefinition
    {
        public string name;
        public string animationTrigger;
        public float damage;
        public float range;
        public float cooldown;
        public bool canBeParried;
        public bool canBeBlocked;
    }

    [Header("Combat Settings")]
    [SerializeField] protected AttackDefinition[] attacks;
    [SerializeField] protected float blockStaminaCost = 10f;
    [SerializeField] protected float parryWindow = 0.2f;
    [SerializeField] protected float executionHealthThreshold = 0.15f;
    
    protected Animator animator;
    protected bool isAttacking;
    protected bool isBlocking;
    protected bool canParry;
    
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public virtual bool CanAttack()
    {
        return !isAttacking && !isBlocking;
    }

    public virtual void PerformAttack(int attackIndex)
    {
        if (!CanAttack() || attackIndex >= attacks.Length) return;

        var attack = attacks[attackIndex];
        isAttacking = true;
        animator.SetTrigger(attack.animationTrigger);
        
        // Implementation specific to AI or player would go in derived classes
    }

    public virtual void StartBlocking()
    {
        if (isAttacking) return;
        isBlocking = true;
        animator.SetBool("IsBlocking", true);
    }

    public virtual void StopBlocking()
    {
        isBlocking = false;
        animator.SetBool("IsBlocking", false);
    }

    public virtual bool AttemptParry()
    {
        if (isAttacking) return false;
        
        canParry = true;
        animator.SetTrigger("Parry");
        StartCoroutine(ParryWindowCoroutine());
        return true;
    }

    protected IEnumerator ParryWindowCoroutine()
    {
        yield return new WaitForSeconds(parryWindow);
        canParry = false;
    }

    public virtual void HandleHit(AttackDefinition attack, Transform attacker)
    {
        if (canParry)
        {
            HandleParry(attack, attacker);
        }
        else if (isBlocking)
        {
            HandleBlock(attack, attacker);
        }
        else
        {
            TakeDamage(attack, attacker);
        }
    }

    protected virtual void HandleParry(AttackDefinition attack, Transform attacker)
    {
        if (!attack.canBeParried) return;
        
        // Play parry animations
        animator.SetTrigger("ParrySuccess");
        if (attacker.TryGetComponent<Animator>(out Animator attackerAnim))
        {
            attackerAnim.SetTrigger("Parried");
        }
        
        OnParrySuccess?.Invoke(attacker);
    }

    protected virtual void HandleBlock(AttackDefinition attack, Transform attacker)
    {
        if (!attack.canBeBlocked) return;
        
        // Implement block logic
        float reducedDamage = attack.damage * 0.2f; // 80% damage reduction
        TakeDamage(new AttackDefinition { damage = reducedDamage }, attacker);
    }

    protected virtual void TakeDamage(AttackDefinition attack, Transform attacker)
    {
        // Implementation in derived classes
    }

    public virtual bool CanBeExecuted()
    {
        var health = GetComponent<IHealth>();
        return health != null && health.GetHealthPercentage() <= executionHealthThreshold;
    }

    public event System.Action<Transform> OnParrySuccess;
}
#endregion

#region AICombatSystem
public class AICombatSystem : CombatSystem2
{
    [SerializeField] private float attackProbability = 0.7f;
    [SerializeField] private float blockProbability = 0.3f;
    [SerializeField] private float parryProbability = 0.2f;
    
    private AIBehaviorController aiController;

    protected override void Awake()
    {
        base.Awake();
        aiController = GetComponent<AIBehaviorController>();
    }

    public void DecideAction()
    {
        if (Random.value < attackProbability && CanAttack())
        {
            PerformAttack(Random.Range(0, attacks.Length));
        }
        else if (Random.value < blockProbability)
        {
            StartBlocking();
        }
        else if (Random.value < parryProbability)
        {
            AttemptParry();
        }
    }

    protected override void TakeDamage(AttackDefinition attack, Transform attacker)
    {
        var health = GetComponent<AIHealth>();
        if (health != null)
        {
            health.TakeDamage(attack.damage);
            
            // Check if should enter execution state
            if (CanBeExecuted())
            {
                aiController.SwitchState(aiController.GetState<AIExecutionState>());
            }
        }
    }
}
#endregion

#region ExecutionSystem2
public class ExecutionSystem2 : MonoBehaviour
{
    [System.Serializable]
    public class ExecutionDefinition
    {
        public string executionerAnimation;
        public string victimAnimation;
        public float duration;
        public float damage;
        public Vector3 positionOffset;
        public Vector3 rotationOffset;
    }

    [SerializeField] private ExecutionDefinition[] executions;
    
    public bool PerformExecution(Transform executor, Transform victim)
    {
        if (!CanExecute(victim)) return false;

        var execution = executions[Random.Range(0, executions.Length)];
        StartCoroutine(ExecutionRoutine(executor, victim, execution));
        return true;
    }

    private bool CanExecute(Transform victim)
    {
        var combatSystem = victim.GetComponent<CombatSystem>();
        return combatSystem != null && combatSystem.CanBeExecuted();
    }

    private IEnumerator ExecutionRoutine(Transform executor, Transform victim, ExecutionDefinition execution)
    {
        // Store original positions
        Vector3 originalExecutorPos = executor.position;
        Vector3 originalVictimPos = victim.position;
        
        // Position both characters
        Vector3 executionPos = victim.position + victim.TransformDirection(execution.positionOffset);
        executor.position = executionPos;
        executor.rotation = victim.rotation * Quaternion.Euler(execution.rotationOffset);

        // Play animations
        executor.GetComponent<Animator>()?.SetTrigger(execution.executionerAnimation);
        victim.GetComponent<Animator>()?.SetTrigger(execution.victimAnimation);

        yield return new WaitForSeconds(execution.duration);

        // Apply damage
        if (victim.TryGetComponent<IHealth>(out var health))
        {
            health.TakeDamage(execution.damage);
        }
    }
}
#endregion

#region AIBehaviorController2
public class AIBehaviorController2 : BaseStateMachine
{
    [Header("AI Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float lowHealthThreshold = 0.15f;

    private AICombatSystem combatSystem;
    private AIHealth health;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        
        // Initialize components
        combatSystem = GetComponent<AICombatSystem>();
        health = GetComponent<AIHealth>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Register states
        RegisterState(new AIPatrolState(this, "Patrol"));
        RegisterState(new AIChaseState(this, "Chase"));
        RegisterState(new AIAttackState(this, "Attack"));
        RegisterState(new AIDeathState(this, "Death"));
        RegisterState(new AIExecutionState(this, "Execution"));

        // Set initial state
        SwitchState(GetState<AIPatrolState>());
    }

    public bool IsPlayerDetected()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle <= fieldOfView * 0.5f)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                return !Physics.Raycast(transform.position, directionToPlayer, 
                    distanceToPlayer, obstacleLayer);
            }
        }
        return false;
    }

    public bool IsLowHealth()
    {
        return health != null && health.GetHealthPercentage() <= lowHealthThreshold;
    }

    public Transform GetPlayer() => player;
    public AICombatSystem GetCombatSystem() => combatSystem;
}
#endregion

#region IStateRegion

public interface IState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public class AIStateFactory
{
    public static IState CreateState(AIState stateType, AIBehaviorController controller)
    {
        switch (stateType)
        {
            case AIState.Patrol:
                return new AIPatrolState(controller);
            case AIState.Chase:
                return new AIChaseState(controller);
            // Add other states...
            default:
                throw new ArgumentException($"State {stateType} not implemented");
        }
    }
}
#endregion

#region CombatSystems
public abstract class BaseCombatSystem3 : MonoBehaviour
{
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float parryWindow = 0.2f;
    [SerializeField] protected float executionThreshold = 0.15f;

    protected Animator animator;
    protected IHealth healthSystem;
    protected bool isAttacking;
    protected bool isBlocking;
    protected bool canParry;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        healthSystem = GetComponent<IHealth>();
    }

    public abstract void PerformAttack(AttackData attackData);
    public abstract void HandleBlock(AttackData attackData);
    public abstract void HandleParry(AttackData attackData);
    public abstract void HandleExecution(Transform target);

    protected bool IsInRange(Transform target, float range)
    {
        return Vector3.Distance(transform.position, target.position) <= range;
    }
}

// Then derive both AI and Player combat systems from this
public class AICombatSystem3 : BaseCombatSystem3
{
    private AIBehaviorController aiController;

    protected override void Awake()
    {
        base.Awake();
        aiController = GetComponent<AIBehaviorController>();
    }

    public override void PerformAttack(AttackData attackData)
    {
        // AI-specific attack implementation
    }
}

public struct AttackData
{
    public float damage;
    public float knockbackForce;
    public bool canBeParried;
    public bool canBeBlocked;
    public string animationTrigger;
    public Transform attacker;
}

public struct ParryData
{
    public float window;
    public float staminaCost;
    public string successAnimation;
    public string failAnimation;
}

public class CombatEvents : MonoBehaviour
{
    public static CombatEvents Instance { get; private set; }

    public event Action<AttackData> OnAttackStarted;
    public event Action<AttackData> OnAttackLanded;
    public event Action<Transform> OnParrySuccessful;
    public event Action<Transform> OnExecutionStarted;
    public event Action<Transform> OnExecutionCompleted;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void TriggerAttackStarted(AttackData data) => OnAttackStarted?.Invoke(data);
    public void TriggerAttackLanded(AttackData data) => OnAttackLanded?.Invoke(data);
    // Add other event triggers
}
#endregion

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private Dictionary<string, int> animationHashes;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        InitializeAnimationHashes();
    }

    private void InitializeAnimationHashes()
    {
        animationHashes = new Dictionary<string, int>
        {
            {"Attack", Animator.StringToHash("Attack")},
            {"Block", Animator.StringToHash("Block")},
            {"Parry", Animator.StringToHash("Parry")},
            {"Execute", Animator.StringToHash("Execute")}
            // Add other animations
        };
    }

    public void PlayAnimation(string animationName, bool useHash = true)
    {
        if (useHash && animationHashes.TryGetValue(animationName, out int hash))
        {
            animator.SetTrigger(hash);
        }
        else
        {
            animator.SetTrigger(animationName);
        }
    }
}*/