using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEditor.Search;
using UnityEngine;
using System.Collections;
using StarterAssets;


public class PlayerController : MonoBehaviour, ICombat
{
    [Header("Refrences")]
    public CharacterController characterController;
    [SerializeField] private Transform mainCamera;

    [Header("Dodge settings")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;
    
    private float lastDodgeTime = -Mathf.Infinity;
    private bool isDodging = false;
    private Vector3 dodgeDirection;

    public InteractableActor currentInteractable;
    bool isDead;
    int level = 1;
    TempPlayerAttributes tempPlayerAttributes;

    
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float sprintTransitSpeed = 5f; // How fast the caracter transitions to sprint
    [SerializeField] private float turningSpeed = 10f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;

    private float verticalVelocity; 
    private float speed;

    [Header("Input")]
    private float moveInput;
    private float turnInput;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        tempPlayerAttributes = GameObject.Find("PlayerStats").GetComponent<TempPlayerAttributes>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isDodging && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            StartCoroutine(Dodge());
        }
        
        InputManagement();
        Movement();

        CheckForInteractable();
        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CallInteract();
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            //GetComponent<InventoryManager>().ToggleInventory();
        }
    }

    private IEnumerator Dodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        float startTime = Time.time;
        dodgeDirection = -transform.forward;

        while (Time.time < startTime + dodgeDuration)
        {
            characterController.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
            yield return null;
        }

        isDodging = false;
    }

    private void CheckForInteractable()
    {
        Vector3 center = new Vector3(Screen.width * .5f, Screen.height * .5f, 0);
        Ray ray = mainCamera.gameObject.GetComponent<Camera>().ScreenPointToRay(center);
        if (Physics.Raycast(ray, out RaycastHit hit, 100))
        {
            if (hit.collider.TryGetComponent<InteractableActor>(out InteractableActor hitActor))
            {
                currentInteractable = hitActor;
            }
            else
                currentInteractable = null;
        }
    }

    private void CallInteract()
    {
        if (currentInteractable.pressToInteract)
        {
            currentInteractable.Interact();
        }
        else
            Debug.Log("touch object instead");
    }

    public int GetLevel() => tempPlayerAttributes.level;

    public void TakeDamage(float incomingDamage, float knockback, Vector3 knockbackDirection)
    {
        if (isDead) return;
        if (GetComponent<CombatManager>().SuccessfullParry())
        {
            Debug.Log("Parried successful");
            return;
        }

        if (GameManager.instance.player.GetCombatManager().isBlocking)
        {
            incomingDamage *= 0.1f;
        }

        tempPlayerAttributes.ModifyHealth(-incomingDamage);
        Debug.Log("Player takes damage" + tempPlayerAttributes.GetFloatAttribute(TempPlayerStats.health));

        if (tempPlayerAttributes.GetFloatAttribute(TempPlayerStats.health) == 0)
            Die();
    }

    public void Die()
    {
        isDead = true;
        Debug.Log($"{this.gameObject.name} Died");
        if (this.transform.root.name == "Player")
        {
            GameManager.instance.KillPlayer();
            TempPlayerAttributes.instance.LevelUp(3);
        }
    }

    public bool IsDead() => this.isDead;
    
    private void Movement()
    {
        GroundMovement();
        Turn();
    }

    private void GroundMovement()
    {
        Vector3 move = new Vector3(moveInput, 0, turnInput);
        move = transform.TransformDirection(move);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = Mathf.Lerp(speed, sprintSpeed, sprintTransitSpeed * Time.deltaTime);
        }
        else
        {
            speed = Mathf.Lerp(speed, walkSpeed, sprintTransitSpeed * Time.deltaTime);

        }

        move *= speed;

        move.y = VerticalForceCalculation();

        characterController.Move(move * Time.deltaTime);
    }

    private void Turn()
    {
        if (Mathf.Abs(turnInput) > 0 || Mathf.Abs(moveInput) > 0)
        {
            Vector3 currentLookDirection = mainCamera.forward;
            currentLookDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(currentLookDirection);

            // Smoothly rotates the player
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);   

        }
    }

    private float VerticalForceCalculation()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = -1;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
        return verticalVelocity;
    }

    private void InputManagement()
    {
        // Get the input from our keyboard
        moveInput = Input.GetAxis("Horizontal");
        turnInput = Input.GetAxis("Vertical");
    }
}
