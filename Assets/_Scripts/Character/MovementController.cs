using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [Header("GroundCheck Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.28f;
    [SerializeField] private float groundOffset = 0.14f;
    
    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 50f;
    [SerializeField] private float minMoveAmount = 0.01f;
    [SerializeField] private float moveSpeed = 50f;
    [SerializeField] private float maxVelocity = 5f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float speedTransitionSpeed = 5f;
    
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 50f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallVelocity = 10f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float terminalVelocity = -50f;
    
    [Header("Step Settings")]
    [SerializeField] private float stepOffset = 0.5f;
    
    [Header("Crouch Settings")]
    [Header("Wall Sliding Settings")]
    
    [Header("Other Settings")]
    [SerializeField] private bool isDebug = false;
    [SerializeField] private float gravity = -9.81f;

    private float _targetSpeed;
    
    private Rigidbody _rb;
    private bool _isGrounded;
    
    private Vector3 _moveDirection;
    private float _speed;
    private Vector3 _velocity;
    private Vector3 _forward;
    private Vector3 _right;
    private Vector3 _targetRotation;
    private Transform cameraTransform;
    
    public bool IsGrounded => _isGrounded;
    public Rigidbody RB => _rb;
    
    public Vector3 Vel => _velocity;

    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
        _rb.useGravity = false;
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
    }
    
    private void FixedUpdate()
    {
        GroundCheck();
        HandleMovement();
        HandleRotation();
        //HandleFalling();
        ApplyGravity();
    }

    private void GroundCheck()
    {
        /* // Sphere trace
        Vector3 spherePosition = transform.position - new Vector3(0, groundOffset, 0);
        _isGrounded = Physics.CheckSphere(spherePosition, groundCheckDistance, groundLayer);*/
        
        Vector3 rayPosition = transform.position - new Vector3(0, groundOffset, 0);
        _isGrounded = Physics.Raycast(rayPosition, Vector3.down, groundCheckDistance, groundLayer);
    }

    public void SetTransforms(Vector3 forward, Vector3 right)
    {
        _forward = forward;
        _right = right;
    }

    public void SetMoveDirection(Vector3 direction, float targetSpeed)
    {
        _moveDirection = direction.normalized;
        //_speed = Mathf.Min(targetSpeed, maxSpeed);
        _targetSpeed = targetSpeed;

        if (direction.magnitude > minMoveAmount)
        {
            _targetRotation = Quaternion.LookRotation(direction).eulerAngles;
        }
    }
    
    public void SetMoveDirection(Vector2 input, float targetSpeed)
    {
        if (input.magnitude < minMoveAmount)
        {
            _moveDirection = Vector3.zero;
            _speed = 0;
            return;
        }
        
        _moveDirection = new Vector3(input.x, 0, input.y).normalized;
        _speed = Mathf.Min(targetSpeed, maxSpeed);
        
        if (input.magnitude > minMoveAmount)
        {
            _targetRotation = Quaternion.LookRotation(input).eulerAngles;
        }
    }

    public void JumpSqrt()
    {
        if (_isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(2 * jumpForce * -gravity);
            Vector3 vel = _rb.linearVelocity;
            vel.y = jumpVelocity;
            _rb.linearVelocity = vel;
        }
    }

    public void SetMovementDirection(Vector3 direction, float speed)
    {
        Vector3 targetVelocity = direction * speed;
        targetVelocity.y = _rb.linearVelocity.y;

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
        if (_rb)
            _rb.linearVelocity = _velocity;
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

        if (_rb.linearVelocity.magnitude < maxVelocity * speed)
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
        /*
        if (_moveDirection.magnitude < minMoveAmount) return;
        
        Vector3 targetVelocity = _moveDirection * _speed;
        targetVelocity.y = _rb.linearVelocity.y;
        
        Vector3 currentVelocity = _rb.linearVelocity;
        Vector3 newVelocity = Vector3.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * 10f);*/
        
        _speed = Mathf.Lerp(_speed, _targetSpeed, speedTransitionSpeed * Time.fixedDeltaTime);
        
        if (_moveDirection.magnitude >= minMoveAmount)
        {
            Vector3 targetVelocity = _moveDirection * _speed;
            targetVelocity.y = _rb.linearVelocity.y;
            
            _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * 10f);
        }
        else
        {
            Vector3 currentVelocity = _rb.linearVelocity;
            currentVelocity.x = Mathf.Lerp(currentVelocity.x, 0, Time.fixedDeltaTime * 10f);
            currentVelocity.z = Mathf.Lerp(currentVelocity.z, 0, Time.fixedDeltaTime * 10f);
            _rb.linearVelocity = currentVelocity;
        }
        //_rb.linearVelocity = newVelocity;
    }
    
    private void HandleRotation()
    {
        if (_rb.linearVelocity.magnitude > minMoveAmount)
        {
            Quaternion targetRotation = Quaternion.Euler(0, _targetRotation.y, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    private void HandleFalling()
    {
        if (_rb.linearVelocity.y < 0)
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (_rb.linearVelocity.y > 0)
        {
            _rb.linearVelocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void ApplyGravity()
    {
        if (!_isGrounded)
        {
            _rb.AddForce(Vector3.up * gravity, ForceMode.Acceleration);
        }
    }

    public void ApplyForce(Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
    {
        if (_rb == null) return;
        _rb.AddForce(direction * force, forceMode);
    }

    private void OnDrawGizmosSelected()
    {
        if (!isDebug) return;
        /*
        Gizmos.color = Color.red;
        Vector3 groundCheckPosition = transform.position - new Vector3(0f, groundOffset, 0f);
        Gizmos.DrawWireSphere(transform.position - new Vector3(0f, groundOffset, 0f), groundCheckDistance);*/
        /*
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 rayStart = transform.position + Vector3.up * groundOffset;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * (groundCheckDistance + groundOffset));*/

        Gizmos.color = Color.yellow;
        Vector3 rayPosition = transform.position - new Vector3(0, groundOffset, 0);
        Gizmos.DrawLine(rayPosition, rayPosition + Vector3.down * groundCheckDistance);
    }
}