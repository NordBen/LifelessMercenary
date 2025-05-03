using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.28f;
    [SerializeField] private float groundCheckOffset = 0.14f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float maxVelocity = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallVelocity = 10f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    
    [Header("Other Settings")]
    [SerializeField] private bool isDebug = false;
    [SerializeField] private float gravity = -9.81f;
    
    private Rigidbody _rb;
    private bool _isGrounded;
    
    private Vector3 _moveDirection;
    private Vector3 _velocity;
    private Transform cameraTransform;
    
    public bool IsGrounded => _isGrounded;

    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        
    }
    
    private void FixedUpdate()
    {
        GroundCheck();
        HandleMovement();
        HandleFalling();
    }

    private void GroundCheck()
    {
        /* // Sphere trace
        Vector3 spherePosition = transform.position - new Vector3(0, groundCheckOffset, 0);
        _isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundLayer);*/
        
        Vector3 rayPosition = transform.position - new Vector3(0, groundCheckOffset, 0);
        _isGrounded = Physics.Raycast(rayPosition, Vector3.down, groundCheckDistance, groundLayer);
    }

    public void SetMovementDirection(Vector3 direction, float speed)
    {
        Vector3 targetVelocity = direction * speed;
        targetVelocity.y = _rb.velocity.y;

        float accelerationRate = direction.magnitude > 0.1f ? acceleration : deceleration;
        
        _velocity = Vector3.MoveTowards(_velocity, targetVelocity, accelerationRate * Time.fixedDeltaTime);

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    private void ApplyVelocity()
    {
        _rb.velocity = _velocity;
    }

    public void Move(Vector2 input, float speed = 1f)
    {
        if (input.magnitude < 0.1f) return;
        
        Vector3 _forward = cameraTransform.forward;
        Vector3 _right = cameraTransform.right;
        _forward.y = 0;
        _right.y = 0;
        _forward.Normalize();
        _right.Normalize();
        
        _moveDirection = (_forward * input.x + _right * input.y);
        _moveDirection.Normalize();

        if (_rb.velocity.magnitude < maxVelocity * speed)
        {
            _rb.AddForce(_moveDirection * moveSpeed * speed, ForceMode.Force);
        }

        if (_moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void Jump()
    {
        if (!_isGrounded) return;
        /*
        _rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);*/
        _velocity.y = jumpForce;
    }
    
    private void HandleMovement()
    {
        
    }
    
    private void HandleFalling()
    {
        if (_rb.velocity.y < 0)
        {
            _rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (_rb.velocity.y > 0)
        {
            _rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    public void ApplyForce(Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
    {
        _rb.AddForce(direction * force, forceMode);
    }

    private void OnDrawGizmosSelected()
    {
        if (!isDebug) return;
        /*
        Gizmos.color = Color.red;
        Vector3 groundCheckPosition = transform.position - new Vector3(0f, groundCheckOffset, 0f);
        Gizmos.DrawWireSphere(transform.position - new Vector3(0f, groundCheckOffset, 0f), groundCheckDistance);*/
        
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * groundCheckOffset;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * (groundCheckDistance + groundCheckOffset));
    }
}