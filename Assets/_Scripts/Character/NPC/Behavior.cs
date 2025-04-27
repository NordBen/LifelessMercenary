using UnityEngine;
using UnityEngine.AI;

public enum EAIState
{
    Idle,
    ChasePlayer,
    SearchForItems,
    Death
}

public interface IAIState
{
    EAIState GetState();
    void Enter(AIAgent agent);
    void Update(AIAgent agent);
    void Exit(AIAgent agent);
}

public class AIStateMachine
{
    public IAIState[] states;
    public EAIState _currentState;
    public AIAgent agent;

    public AIStateMachine(AIAgent agent)
    {
        this.agent = agent;
        int numStates = System.Enum.GetNames(typeof(EAIState)).Length;
        states = new IAIState[numStates];
    }

    public void RegisterState(IAIState state)
    {
        int index = (int)state.GetState();
        states[index] = state;
    }

    public IAIState GetCurrentState()
    {
        return states[(int)_currentState];
    }
    
    public void Update()
    {
        GetCurrentState()?.Update(agent);
    }

    public void ChangeState(EAIState newState)
    {
        GetCurrentState()?.Exit(agent);
        _currentState = newState;
        GetCurrentState()?.Enter(agent);
    }
}

public class ChasePlayerState : IAIState
{
    public float timer = 0f;
    
    public EAIState GetState()
    {
        return EAIState.ChasePlayer;
    }
    
    public void Enter(AIAgent agent)
    {
        
    }
    
    public void Update(AIAgent agent)
    {
        if (!agent.enabled) return;

        timer -= Time.deltaTime;
        if (agent.navMeshAgent.hasPath) agent.navMeshAgent.destination = agent.playerTransform.position;

        if (timer < 0f)
        {
            Vector3 direction = (agent.playerTransform.position - agent.navMeshAgent.destination);
            direction.y = 0f;
            if (direction.sqrMagnitude > agent.maxDistance * agent.maxDistance)
            {
                if (agent.navMeshAgent.pathStatus != NavMeshPathStatus.PathPartial)
                {
                    agent.navMeshAgent.destination = agent.playerTransform.position;
                }
            }
        }
        timer = agent.maxTime;
    }

    public void Exit(AIAgent agent)
    {
        
    }
}

public class DeathState : IAIState
{
    public Vector3 direction = Vector3.zero;
    
    public EAIState GetState()
    {
        return EAIState.Death;   
    }

    public void Enter(AIAgent agent)
    {
        agent.ragdoll.ActivateRagdoll();
        direction.y = 1;
        agent.ragdoll.ApplyForce(direction * agent.dieForce);
        agent.healthBar.gameObject.SetActive(false);
        agent.mesh.updateWhenOffscreen = true;
    }

    public void Update(AIAgent agent)
    {
    }

    public void Exit(AIAgent agent)
    {
    }
}

public class AIHealthComponent : MonoBehaviour, ICombat
{
    public float health = 100f;
    public float maxHealth = 100f;
    
    AIAgent agent;

    public void TakeDamage(float incomingDamage, float knockbackForce, Vector3 direction)
    {
        this.health -= incomingDamage;
        
        if (this.health <= 0)
            Die(direction);
    }

    public void Die() { }

    public void Die(Vector3 direction)
    {
        DeathState deathState = agent.stateMachine.states[(int)EAIState.Death] as DeathState;
        if (deathState != null)
        {
            deathState.direction = direction;
            agent.stateMachine.ChangeState(EAIState.Death);
        }
    }

    public bool IsDead() => this.health <= 0;
    public int GetLevel() => 1;
}

public class IdleState : IAIState
{
    public EAIState GetState()
    {
        return EAIState.Idle;  
    }

    public void Enter(AIAgent agent)
    {
        
    }

    public void Update(AIAgent agent)
    {
        Vector3 playerDirection = (agent.playerTransform.position - agent.transform.position);
        if (playerDirection.magnitude > agent.maxSightDistance) return;
        
        Vector3 agentDirection = agent.transform.forward;
        
        playerDirection.Normalize();
        
        float dotProduct = Vector3.Dot(playerDirection, agentDirection);
        
        if (dotProduct > 0f)
            agent.stateMachine.ChangeState(EAIState.ChasePlayer);
    }

    public void Exit(AIAgent agent)
    {
        
    }
}

public class FindWeaponState : IAIState
{
    private InteractableEquipment pickup;
    
    public EAIState GetState()
    {
        return EAIState.SearchForItems;
    }

    public void Enter(AIAgent agent)
    {
        pickup = FindClosestWeapon(agent);
        agent.navMeshAgent.destination = pickup.transform.position;
        agent.navMeshAgent.speed = 5;
    }

    public void Update(AIAgent agent)
    {
        if (agent.transform.position == agent.navMeshAgent.destination)
        {
            pickup.Interact();
        }
    }

    public void Exit(AIAgent agent)
    {
    }

    private InteractableEquipment FindClosestWeapon(AIAgent agent)
    {
        InteractableEquipment[] weapons = UnityEngine.Object.FindObjectsOfType<InteractableEquipment>();
        InteractableEquipment closestWeapon = null;
        float closestDistance = float.MaxValue;
        foreach (var weapon in weapons)
        {
            float distanceToWeapon = Vector3.Distance(agent.transform.position, weapon.transform.position);
            if (distanceToWeapon < closestDistance)
            {
                closestDistance = distanceToWeapon;
                closestWeapon = weapon;
            }
        }
        return closestWeapon;
    }
}