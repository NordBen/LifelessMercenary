using LM.UI;
using UnityEngine;
using UnityEngine.AI;

public class AIAgent : MonoBehaviour
{
    public Transform playerTransform;
    
    public AIStateMachine stateMachine;
    public EAIState initialState;
    
    [Header("===Settings==")]
    public float maxTime = 1f;
    public float maxDistance = 1f;
    public float dieForce = 10f;
    public float maxSightDistance = 5f;
    public NavMeshAgent navMeshAgent;

    public Ragdoll ragdoll;
    public SkinnedMeshRenderer mesh;
    public ValueProgressBar healthBar;
    
    private void Start()
    {
        ragdoll = GetComponent<Ragdoll>();
        mesh = GetComponentInChildren<SkinnedMeshRenderer>();
        healthBar = GetComponentInChildren<ValueProgressBar>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        
        stateMachine = new AIStateMachine(this);
        RegisterStates();
    }
    
    void Update()
    {
        stateMachine.Update();
    }

    private void RegisterStates()
    {
        stateMachine.RegisterState(new ChasePlayerState());
        stateMachine.RegisterState(new DeathState());
        stateMachine.RegisterState(new IdleState());
        stateMachine.ChangeState(initialState);
    }
}