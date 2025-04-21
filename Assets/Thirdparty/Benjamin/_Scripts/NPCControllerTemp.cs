using UnityEngine;

public class NPCControllerTemp : BaseCharacter
{
    // Animator parameters - Allows us to set the parameters in the Animator Controller without referring to them by string
    private static readonly int IsIdle = Animator.StringToHash("isIdle");
    private static readonly int IsWalking = Animator.StringToHash("isWalking");
    private static readonly int IsChasing = Animator.StringToHash("isChasing");
    private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
    private static readonly int IsSkill = Animator.StringToHash("isSkill");

    // Constants
    private const float AccuracyWp = 0.5f; // Accuracy for the NPC to reach the waypoint

    // Inspector variables
    [Header("NPC Settings")]
    [Tooltip("Show Gizmo for debugging purposes")]
    [SerializeField] private bool bShowGizmo;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform npcHead;
    [Tooltip("Waypoints for the NPC to follow")]
    [SerializeField] private Transform[] wayPoint;
    [Tooltip("Field of view angle for the NPC")]
    [Range(1f, 179f)]
    [SerializeField] private float fieldOfViewAngle = 120f;
    [Tooltip("Rotation speed for the NPC")]
    [Range(0.1f, 5)]
    [SerializeField] private float rotationSpeed = 1.2f;
    [Tooltip("Chase distance for the NPC")]
    [Range(1f, 50f)]
    [SerializeField] private float chaseDistance = 5f;
    [Tooltip("Attack distance for the NPC")]
    [Range(1f, 10f)]
    [SerializeField] private float attackDistance = 1.5f;

    // Private variables for the NPC's internal state handling
    private int _currentWp;
    private float fieldOfViewHalfAngle;
    private bool hyperSensitive = false;

// Enum for selecting NPC behavior pattern
    private enum ENPCType {
        Normal, Agressive, Persistent, Commander
    }

    private enum EState
    {
        Patrol, Chase, Attack, Skill, Dead, Hit
    }
    [Tooltip("Current state of the NPC")]
    [SerializeField]
    private EState _currentState = EState.Patrol;
    [Tooltip("Type of NPC")]
    [SerializeField]
    private ENPCType _npcType = ENPCType.Normal;

    private void Start()
    {
        SetupNPC();
        base.Start(); // Run the base logic from BaseCharacter
        // add the enemy to the gamemanagers list of enemies if they are not a commander
        if (this._npcType != ENPCType.Commander)
            GameManager.instance.AddEnemy(this); 
    }

    private void Update()
    {
        base.Update();
        if (this.isDead) return;
        // Calculate the direction to the player
        var direction = playerTransform.position - transform.position;
        direction.y = 0f;

        // Calculate the angle between the NPC's forward vector and the direction to the player
        var angleToPlayer = Vector3.Angle(direction, npcHead.up);

        // If the NPC is patrolling, move towards the current waypoint
        if (_currentState==EState.Patrol && wayPoint.Length > 0)
        {
            // If the NPC is close to the current waypoint, switch to the next waypoint
            // If the NPC has reached the last waypoint, reset to the first waypoint
            if (Vector3.Distance(wayPoint[_currentWp].position, transform.position) < AccuracyWp)
            {
                _currentWp++;
                if (_currentWp >= wayPoint.Length)
                    _currentWp = 0;
            }
        
            // Calculate the direction to the current waypoint
            direction = wayPoint[_currentWp].position - transform.position;
            direction.y = 0f;

            // Rotate and move towards the current waypoint
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            transform.Translate(0f, 0f, Time.deltaTime * this._currentSpeed, Space.Self);
        }

        // If the NPC can see the player, chase the player, setting the current state of the NPC to chase
        // If the NPC is close enough to the player, attack the player, setting the current state of the NPC to attack
        if (Vector3.Distance(playerTransform.position, transform.position) < chaseDistance 
            && (angleToPlayer < fieldOfViewHalfAngle || _currentState == EState.Chase || _currentState == EState.Attack))
        {
            // If the NPC is chasing the player, set the speed to run speed
            ChangeAnimState(EState.Chase);
            this._currentSpeed = this.runSpeed;

            // Rotate and move towards the player
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            if (direction.magnitude > attackDistance)
            {
                // If the NPC is not close enough to the player, move towards the player
                transform.Translate(0f, 0f, Time.deltaTime * this._currentSpeed);
            } else // If the NPC is close enough to the player, attack the player
            {
                ChangeAnimState(EState.Attack);
            }
        } else // If the NPC cannot see the player, meaning it is too far away or not within the NPC's field of view, set the current state to patrol and reduce speed to walking speed
        {
            ChangeAnimState(EState.Patrol);
            this._currentSpeed = this.walkSpeed;
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a (green) line from the NPC to the current waypoint if patrolling
        // Draw a (red) line from the NPC to the player if chasing or attacking
        if (bShowGizmo)
        {
            Vector3 direction;
            if (_currentState == EState.Patrol)
            {
                Gizmos.color = Color.green;
                direction = wayPoint[_currentWp].position - transform.position;
            }
            else
            {
                Gizmos.color = Color.red;
                direction = playerTransform.position - transform.position;
            }
            Gizmos.DrawLine(transform.position, transform.position + direction);
        }
    }

    public override void TakeDamage(float incomingDamage, float knockbackForce, Vector3 knockbackDirection)
    {
        // return early and not do any logic if already dead
        if (this.isDead) return;

        ChangeAnimState(EState.Hit);
        // checks if the player is behind the npc and if it is deals double damage and half the knockback but if the player is infront of the npc deals normal damage and knockback
        if (Vector3.Dot(transform.forward, (playerTransform.position - transform.position).normalized) < 0)
        {
            Debug.Log($"Got backstabbed");
            incomingDamage *= 2f;
            knockbackForce *= 0.5f;
        }
        base.TakeDamage(incomingDamage, knockbackForce, knockbackDirection);

        if (this.hyperSensitive)
            ChangeAnimState(EState.Chase);

        // calls for a random reinforcement if the NPC is a commander
        if (_npcType == ENPCType.Commander)
        {
            NPCControllerTemp reinforcement = GameManager.instance.GetRandomEnemy();
            reinforcement.chaseDistance = 999;
            reinforcement.ChangeAnimState(EState.Chase);
        }
    }

    public override void Die()
    {
        // removes the enemy after it dies from the enemy list so that it is not available for being called as a reinforcement and plays the death animation and deletes itself after 5 seconds
        base.Die();
        ChangeAnimState(EState.Dead);
        GameManager.instance.RemoveEnemy(this);
        Invoke("Destroy()", 8f);
    }

    private void SetupNPC()
    {
        SkinnedMeshRenderer _rend = transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
        switch (this._npcType)
        {
            case ENPCType.Normal:
                this.hyperSensitive = false;
                _rend.material.color = Color.white;
                break;
            case ENPCType.Agressive:
                // increases the speed of Brutes and doubles their damage but makes their health slightly lower for balancing their higher damage output
                this.walkSpeed *= 1.5f;
                this.runSpeed *= 2f;
                this.damage *= 2f;
                this.maxHP *= 0.8f;
                this.hyperSensitive = true;
                _rend.material.color = Color.red;
                break;
            case ENPCType.Persistent:
                // makes persistent NPC's have wider detection angle, range and increases their health so they are slightly harder to kill
                this.maxHP *= 1.25f;
                this.chaseDistance *= 2f;
                this.fieldOfViewAngle += 50;
                this.hyperSensitive = false;
                _rend.material.color = Color.blue;
                break;
            case ENPCType.Commander:
                // increases the maximum hit points for commanders
                this.maxHP *= 1.5f;
                this.hyperSensitive = true;
                _rend.material.color = Color.yellow;
                break;
            default:
                break;
        }
        fieldOfViewHalfAngle = fieldOfViewAngle * 0.5f; // end of the method to give the Persistent NPC time to increase their FieldOfViewAngle
    }

    void ChangeAnimState(EState newState)
    {
        // Sets the current state to the new state, and makes sure all bools are reset before setting the needed animation bool to true based on state
        _currentState = newState;
        _animator.SetBool(IsIdle, false);
        _animator.SetBool(IsWalking, false);
        _animator.SetBool(IsChasing, false);
        _animator.SetBool(IsAttacking, false);

        switch (_currentState)
        {
            case EState.Patrol:
                _animator.SetBool(IsWalking, true);
                break;
            case EState.Chase:
                _animator.SetBool(IsChasing, true);
                break;
            case EState.Attack:
                _animator.SetBool(IsAttacking, true);
                break;
            case EState.Dead:
                _animator.SetTrigger("tDead");
                break;
            case EState.Hit:
                _animator.SetTrigger("tHit");
                break;
            default:
                break;
        }
    }
}