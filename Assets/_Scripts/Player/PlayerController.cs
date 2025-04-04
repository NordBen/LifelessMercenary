using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refrences")]
    private CharacterController characterController;
    [SerializeField] private Transform mainCamera;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float turningSpeed = 10f;

    [Header("Input")]
    private float moveInput;
    private float turnInput;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        InputManagement();
        Movement();
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

        move.y = 0;

        move *= walkSpeed;

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

    private void InputManagement()
    {
        // Get the input from our keyboard
        moveInput = Input.GetAxis("Horizontal");
        turnInput = Input.GetAxis("Vertical");
    }
}
