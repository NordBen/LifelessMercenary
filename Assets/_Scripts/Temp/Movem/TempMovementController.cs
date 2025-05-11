using System;
using UnityEngine;

public class TempMovementController : MonoBehaviour
{
    [Header("Movement settings")]
    public float movementSpeed = 5f;
    public float airControl = 2f;
    public float jumpSpeed = 10f;
    public float jumpDuration = 0.2f;
    public float airFirction = 0.5f;
    public float groundFriction = 100f;
    public float gravity = 30f;
    public float slopeLimit = 30f;
    public bool useLocalMomentum = false;
    private float moveSpeedMultiplier = 1f;

    private Vector3 momentum, savedVelocity, savedMovementVelocity;

    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnLand = delegate { };

    [Header("Collider Settings:")]
    [Range(0f, 1f)][SerializeField] float stepHeightRatio = 0.15f;
    [SerializeField] float colliderHeight = 2f;
    [SerializeField] float colliderThickness = 1f;
    [SerializeField] Vector3 colliderOffset = Vector3.zero;

    private Rigidbody rb;
    private Transform tr;
    private CapsuleCollider col;
    private GroundCast groundSensor;
    private CeilingDetector ceilingDetector;
    private ExtStateMachine stateMachine;

    private bool jumpKeyIsPressed;
    private bool jumpKeyWasPressed;
    private bool jumpKeyWasLetGo;
    private bool jumpInputIsLocked;
    private CountdownTimer jumpTimer;

    private bool isGrounded;
    private float baseSensorRange;
    private Vector3 currentGroundAdjustmentVelocity;
    private int currentLayer;
    
    private bool useFixedTimestep = true;
    
    [Header("Rotation Settings")]
    public float rotationSmoothTime = 0.12f;
    public bool enableRotation = true;
    private float rotationVelocity;
    private Vector3 targetRotationDirection;
    private bool hasRotationTarget = false;

    [Header("Sensor Settings")]
    [SerializeField] bool useDebug;
    private bool isUsingExtendedSensorRange = true;

    private void Awake()
    {
        Setup();
        RecalculateColliderDimensions();
        SetupStateMachine();
        jumpTimer = new CountdownTimer(jumpDuration);
    }
    
    public void SetRotationTarget(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            targetRotationDirection = direction.normalized;
            hasRotationTarget = true;
        }
    }
    
    private void ApplyRotation()
    {
        if (!enableRotation || !hasRotationTarget)
            return;
        
        // Make sure we only rotate horizontally
        targetRotationDirection.y = 0;
    
        // Calculate the target angle from the direction
        float targetAngle = Mathf.Atan2(targetRotationDirection.x, targetRotationDirection.z) * Mathf.Rad2Deg;
    
        // Get the current rotation angle
        float currentAngle = tr.eulerAngles.y;
    
        // Smoothly rotate towards the target using SmoothDampAngle
        float newAngle = Mathf.SmoothDampAngle(
            currentAngle, 
            targetAngle, 
            ref rotationVelocity, 
            rotationSmoothTime,
            float.MaxValue,
            Time.fixedDeltaTime);
    
        // Apply the rotation using Rigidbody's MoveRotation which works with physics
        Quaternion targetRotation = Quaternion.Euler(0, newAngle, 0);
        rb.MoveRotation(targetRotation);
    }

    private void OnValidate()
    {
        if (gameObject.activeInHierarchy)
            RecalculateColliderDimensions();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdate();
        CheckForGround();
        HandleMomentum();
        Vector3 velocity = CalculateMovementVelocity();
        velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

        SetExtendedSensorRange(IsGrounded());
        SetVelocity(velocity);
        
        ApplyRotation();

        savedVelocity = velocity;
        savedMovementVelocity = CalculateMovementVelocity();

        if (!ceilingDetector) ceilingDetector.Reset();
    }

    private void Update()
    {
        stateMachine.Update();
        if (Input.GetKeyDown(KeyCode.V))
        {
            OnJumpStart();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            HandleJumping();
        }
    }

    private void LateUpdate()
    {
        if (useDebug)
            groundSensor.DrawDebug();
    }

    private void Setup()
    {
        tr = transform;
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        ceilingDetector = GetComponent<CeilingDetector>();
    }

    public void ApplyForce(Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
    {
        if (rb == null) return;
        rb.AddForce(direction * force, forceMode);
    }

    public void MoveFastToward(float dashForce, float dashUp)
    {
        Vector3 dir = GetMovementVelocity();

        Vector3 forceToApp = transform.forward * dashForce + transform.up * dashUp;
        rb.AddForce(forceToApp, ForceMode.Impulse);
    }

    public void Dash(float dashForce, float inCooldown)
    {
        moveSpeedMultiplier = dashForce;
        Invoke("ResetDash", inCooldown);
    }

    private void ResetDash()
    {
        moveSpeedMultiplier = 1f;
    }
    
    void SetupStateMachine()
    {
        stateMachine = new ExtStateMachine();

        var grounded = new GroundedState(this);
        var falling = new FallingState(this);
        var sliding = new SlidingState(this);
        var rising = new RisingState(this);
        var jumping = new JumpingState(this);

        At(grounded, rising, () => IsRising());
        At(grounded, sliding, () => IsGrounded() && IsGroundTooSteep());
        At(grounded, falling, () => !IsGrounded());
        At(grounded, jumping, () => (jumpKeyIsPressed || jumpKeyWasPressed) && !jumpInputIsLocked);

        At(falling, rising, () => IsRising());
        At(falling, grounded, () => IsGrounded() && !IsGroundTooSteep());
        At(falling, sliding, () => IsGrounded() && IsGroundTooSteep());

        At(sliding, rising, () => IsRising());
        At(sliding, falling, () => !IsGrounded());
        At(sliding, grounded, () => IsGrounded() && !IsGroundTooSteep());

        At(rising, grounded, () => IsGrounded() && !IsGroundTooSteep());
        
        At(rising, sliding, () => IsGrounded() && IsGroundTooSteep());
        At(rising, falling, () => IsFalling());
        At(rising, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());

        At(jumping, rising, () => jumpTimer.isFinished || jumpKeyWasLetGo);
        At(jumping, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());

        stateMachine.SetState(falling);
    }

    #region movement
    public Vector3 GetVelocity() => savedVelocity;
    public Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
    public Vector3 GetMovementVelocity() => savedMovementVelocity;

    private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * (movementSpeed * moveSpeedMultiplier);
    public Vector3 CalculateMovementDirection()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector3 direction = Camera.main.transform == null ?
            tr.right * input.x + tr.forward * input.y
            : Vector3.ProjectOnPlane(Camera.main.transform.right, tr.up).normalized * input.x + Vector3.ProjectOnPlane(Camera.main.transform.forward, tr.up).normalized * input.y;

        return direction.magnitude > 1f ? direction.normalized : direction;
    }

    private void HandleMomentum()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
        Vector3 horizontalMomentum = momentum - verticalMomentum;

        verticalMomentum -= tr.up * (gravity * Time.deltaTime);
        if (isGrounded && VectorMath.GetDotProduct(verticalMomentum, tr.up) < 0f) verticalMomentum = Vector3.zero;

        if (!isGrounded) 
            AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());

        float friction = isGrounded ? groundFriction : airFirction;
        horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);

        momentum = horizontalMomentum + verticalMomentum;

        if (stateMachine.CurrentState is JumpingState) 
            HandleJumping(); // jumping

        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    void At(IStateS2 from, IStateS2 to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
    void Any<T>(IStateS2 to, Func<bool> condition) => stateMachine.AddAnyTransition(to, condition);
    private bool IsRising() => VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0;
    private bool IsFalling() => VectorMath.GetDotProduct(GetMomentum(), tr.up) < 0;
    private bool IsGroundTooSteep() => !IsGrounded() || Vector3.Angle(GetGroundNormal(), tr.up) > slopeLimit;

    void HandleJumpKeyInput(bool isButtonPressed)
    {
        if (!jumpKeyIsPressed && isButtonPressed)
        {
            jumpKeyWasPressed = true;
        }

        if (jumpKeyIsPressed && !isButtonPressed)
        {
            jumpKeyWasLetGo = true;
            jumpInputIsLocked = false;
        }

        jumpKeyIsPressed = isButtonPressed;
    }

    private void HandleJumping()
    {
        momentum = VectorMath.RemoveDotVector(momentum, tr.up);
        momentum += tr.up * jumpSpeed;
    }
    
    private void ResetJumpKeys()
    {
        jumpKeyWasLetGo = false;
        jumpKeyWasPressed = false;
    }

    public void OnJumpStart()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        momentum += tr.up * jumpSpeed;
        jumpTimer.Start();
        jumpInputIsLocked = true;
        OnJump?.Invoke(momentum);

        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    public void OnFallStart()
    {
        var currentUpMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
        momentum = VectorMath.RemoveDotVector(momentum, tr.up);
        momentum -= tr.up * currentUpMomentum.magnitude;
    }

    private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentumVector, Vector3 movementVelocityVector)
    {
        if (horizontalMomentumVector.magnitude > (movementSpeed * moveSpeedMultiplier))
        {
            if (VectorMath.GetDotProduct(movementVelocityVector, horizontalMomentumVector.normalized) > 0f)
            {
                movementVelocityVector = VectorMath.RemoveDotVector(movementVelocityVector, horizontalMomentumVector.normalized);
            }
            horizontalMomentumVector += movementVelocityVector * (Time.deltaTime * airControl * 0.25f);
        }
        else
        {
            horizontalMomentumVector += movementVelocityVector * (Time.deltaTime * airControl);
            horizontalMomentumVector = Vector3.ClampMagnitude(horizontalMomentumVector, (movementSpeed * moveSpeedMultiplier));
        }
    }

    #endregion
    #region GroundCheck
    public void CheckForGround()
    {
        if (currentLayer != gameObject.layer)
            RecalculateColliderDimensions();

        currentGroundAdjustmentVelocity = Vector3.zero;
        float colHeightScale = colliderHeight * tr.localScale.x;

        groundSensor.castLength = isUsingExtendedSensorRange ? 
            baseSensorRange + colHeightScale * stepHeightRatio 
            : baseSensorRange;
        groundSensor.Cast();

        isGrounded = groundSensor.HasDetectedHit();
        if (!isGrounded) return;

        float distance = groundSensor.GetDistance();
        float upperLimit = colHeightScale * (1f - stepHeightRatio) * 0.5f;
        float middle = upperLimit + colHeightScale * stepHeightRatio;
        float distanceToGo = middle - distance;

        currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
    }

    public void OnGroundContactLost()
    {
        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;

        Vector3 velocity = GetMovementVelocity();
        if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
        {
            Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);
            float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

            if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) velocity = Vector3.zero;
            else if (dot > 0f) velocity -= projectedMomentum;
        }
        momentum += velocity;

        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    public void OnGroundContactRegained()
    {
        Vector3 collisionVelocity = useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
        OnLand?.Invoke(collisionVelocity);
    }

    public bool IsGrounded() => isGrounded;
    private bool IsGrounded(ExtStateMachine statMach) => statMach.CurrentState is GroundedState or SlidingState;
    public Vector3 GetGroundNormal() => groundSensor.GetNormal();

    public void SetVelocity(Vector3 velocity) => rb.linearVelocity = velocity + currentGroundAdjustmentVelocity;

    public void SetExtendedSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;

    private void RecalculateColliderDimensions()
    {
        if (!col) Setup();

        col.height = colliderHeight * (1f - stepHeightRatio);
        col.radius = colliderThickness * 0.5f;
        col.center = colliderOffset * colliderHeight + new Vector3(0, 0, 0);

        float colHalfHeight = col.height * 0.5f;
        if (colHalfHeight < col.radius)
            col.radius = colHalfHeight;

        RecalibrateGroundSensor();
    }

    private void RecalibrateGroundSensor()
    {
        groundSensor ??= new GroundCast(tr);

        groundSensor.SetCastOrigin(col.bounds.center);
        groundSensor.SetCastDirection(GroundCast.CastDirection.Down);
        RecalculateGroundSensorLayerMask();

        const float safetyDistanceFactor = 0.001f;

        float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
        baseSensorRange = length * (1f + safetyDistanceFactor) * tr.localScale.x;
        groundSensor.castLength = length * tr.localScale.x;
    }

    private void RecalculateGroundSensorLayerMask()
    {
        int objectLayer = gameObject.layer;
        int layerMask = Physics.AllLayers;

        for (int i = 0; i < layerMask; i++)
        {
            if (Physics.GetIgnoreLayerCollision(objectLayer, i))
                layerMask &= ~(1 << i);
        }

        int ignoredRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        layerMask &= ~(1 << ignoredRaycastLayer);

        groundSensor.layermask = layerMask;
        currentLayer = objectLayer;
    }
    #endregion
}

public class GroundCast
{
    public float castLength = 1f;
    public LayerMask layermask = 255;

    Vector3 origin = Vector3.zero;
    Transform transform;

    public enum CastDirection { Forward, Right, Up, Backward, Left, Down }
    CastDirection castDirection;

    RaycastHit hitInfo;

    public GroundCast(Transform ownerTransform)
    {
        transform = ownerTransform;
    }

    public void Cast()
    {
        Vector3 worldOrigin = transform.TransformPoint(origin);
        Vector3 worldDirection = GetCastDirection();

        Physics.Raycast(worldOrigin, worldDirection, out hitInfo, castLength, layermask, QueryTriggerInteraction.Ignore);
    }

    public bool HasDetectedHit() => hitInfo.collider != null;
    public float GetDistance() => hitInfo.distance;
    public Vector3 GetNormal() => hitInfo.normal;
    public Vector3 GetPosition() => hitInfo.point;
    public Collider GetCollider() => hitInfo.collider;
    public Transform GetTransform() => hitInfo.transform;

    public void SetCastDirection(CastDirection direction) => castDirection = direction;
    public void SetCastOrigin(Vector3 pos) => origin = transform.InverseTransformPoint(pos);

    Vector3 GetCastDirection()
    {
        return castDirection switch
        {
            CastDirection.Forward => transform.forward,
            CastDirection.Right => transform.right,
            CastDirection.Up => transform.up,
            CastDirection.Backward => -transform.forward,
            CastDirection.Left => -transform.right,
            CastDirection.Down => -transform.up,
            _ => Vector3.one
        };
    }

    public void DrawDebug()
    {
        if (!HasDetectedHit()) return;

        Debug.DrawRay(hitInfo.point, hitInfo.normal, Color.red, Time.deltaTime);
        float markerSize = .2f;
        Debug.DrawLine(hitInfo.point + Vector3.up * markerSize, hitInfo.point - Vector3.up * markerSize, Color.green, Time.deltaTime);
        Debug.DrawLine(hitInfo.point + Vector3.right * markerSize, hitInfo.point - Vector3.right * markerSize, Color.green, Time.deltaTime);
        Debug.DrawLine(hitInfo.point + Vector3.forward * markerSize, hitInfo.point - Vector3.forward * markerSize, Color.green, Time.deltaTime);
    }
}