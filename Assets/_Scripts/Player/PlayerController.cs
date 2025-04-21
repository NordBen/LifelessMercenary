using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEditor.Search;
using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && !isDodging && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            StartCoroutine(Dodge());
        }

        InputManagement();
        Movement();
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
