using UnityEngine;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;

public class PlayerControllerV2 : MonoBehaviour, ICombat
{
    private readonly int _animIDSpeed = Animator.StringToHash("Speed");
    private readonly int _animIDGrounded = Animator.StringToHash("Grounded");
    private readonly int _animIDJump = Animator.StringToHash("Jump");
    private readonly int _animIDFreeFall = Animator.StringToHash("FreeFall");
    private readonly int _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    
    [Header("Refrences")]
    [SerializeField] private Transform mainCamera;
    private MovementController _movementController;
    [SerializeField] private Rigidbody[] ragdollRigidBodies;
    [SerializeField] private Collider[] ragdollColliders;
    /*
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;
    private float _currentSpeed;
    [SerializeField] private float sprintTransitSpeed = 5f;
    [SerializeField] private float turningSpeed = 10f;
    [SerializeField] private float gravity = 9.81f;*/
    [SerializeField] private float jumpHeight = 2f;

    [Header("Dodge settings")]
    [SerializeField] private float dodgeSpeed = 10f;
    [SerializeField] private float dodgeDuration = 0.2f;
    [SerializeField] private float dodgeCooldown = 1f;

    private float lastDodgeTime = 0;
    private bool isDodging = false;

    public InteractableActor currentInteractable;
    bool isDead;
    int level = 1;
    //TempPlayerAttributes tempPlayerAttributes;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;
    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;
    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    private float verticalVelocity;
    private float speed;

    private float _fallTimeoutDelta;

    private PlayerInput _playerInput;
    private Animator _animator;
    private StarterAssetsInputs _input;
    private float _animationBlend;
    private bool _hasAnimator;
    
    private Vector2 moveInput;

    private const float _threshold = 0.01f;
    
    private bool _IsGrounded = true;

    private bool isDeady = false;
    
    [Header("Rotation Settings")]
    [SerializeField] private float cameraRotationInfluence = 0.5f;
    [SerializeField] private float movementRotationInfluence = 0.5f;

    public TempMovementController _mc;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
        }
    }

    #region Unity
    private void Awake()
    {
    }

    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
        
        ToggleRagdolls();
        _hasAnimator = TryGetComponent(out _animator);
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();

        //_movementController = GetComponent<MovementController>();
        //tempPlayerAttributes = GameObject.Find("PlayerStats").GetComponent<TempPlayerAttributes>();

        _fallTimeoutDelta = FallTimeout;
    }

    #region Update
    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        _IsGrounded = _mc.IsGrounded();// _movementController.IsGrounded;
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _IsGrounded);
        }
        
        Vector3 rotationDirection = Vector3.zero;
        
        if (_input.move.magnitude > 0.1f)
        {
            // Get camera forward (excluding vertical component)
            Vector3 cameraForward = Vector3.ProjectOnPlane(mainCamera.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(mainCamera.right, Vector3.up).normalized;
        
            // Calculate direction relative to camera view
            rotationDirection = (cameraForward * _input.move.y + cameraRight * _input.move.x).normalized;
        
            // Pass this direction to the movement controller
            _mc.SetRotationTarget(rotationDirection);
        }
        
        if (_mc.GetVelocity().sqrMagnitude < _threshold)
        { _animationBlend = 0f; }
        else
        { _animationBlend = Input.GetKey(KeyCode.LeftShift) ? 6 : 2.5f; }

        if (_IsGrounded)
        {
            _fallTimeoutDelta = FallTimeout;
            if (_hasAnimator) 
            { _animator.SetBool(_animIDJump, false); 
                _animator.SetBool(_animIDFreeFall, false); }
        } 
        else 
        {
            if (_fallTimeoutDelta >= 0.0f) { _fallTimeoutDelta -= Time.deltaTime; }
            else { if (_hasAnimator) { _animator.SetBool(_animIDFreeFall, true); } } 
        }
        if (_hasAnimator)
        { float inputMagnitude = 1;
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude); }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartCoroutine(DodgeTesti());
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetKeyDown(KeyCode.Y))
        {
            isDeady = !isDeady;
            _animator.enabled = isDeady;
            ToggleRagdolls(isDeady);
            //_animator.SetBool("bIsDead", isDeady);
        }
        
        if (Input.GetKeyDown(KeyCode.U))
        {
            _animator.SetBool("bIsDead", true);
        }
        
        //Movement();

        CheckForInteractable();
        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                CallInteract();
            }
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    #endregion
    #endregion
    #region Interaction
    private void CheckForInteractable()
    {
        Vector3 center = new Vector3(Screen.width * .5f, Screen.height * .5f, 0);
        Ray ray = Camera.main.ScreenPointToRay(center); //mainCamera.gameObject.GetComponent<Camera>().ScreenPointToRay(center);
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

    private void ToggleRagdolls()
    {
        foreach (var rb in ragdollRigidBodies)
        {
            rb.isKinematic = !rb.isKinematic;
        }/*
        foreach (var collider in ragdollColliders)
        {
            collider.enabled = !collider.enabled;
        }*/
    }
    
    private void ToggleRagdolls(bool toggle)
    {
        foreach (var rb in ragdollRigidBodies)
        {
            rb.isKinematic = !toggle;
        }/*
        foreach (var collider in ragdollColliders)
        {
            collider.enabled = !toggle;
        }*/
    }

    private IEnumerator DodgeTesti()
    {
        isDodging = true;
        lastDodgeTime = Time.time;
    
        float startTime = Time.time;
        Vector3 dodgeDirection;
    
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (moveInput != Vector2.zero)
        {
            dodgeDirection = transform.forward;
        }
        else
        {
            dodgeDirection = -transform.forward;
        }
    
        float dodgeForce = moveInput.magnitude > _threshold ? dodgeSpeed * 3 : dodgeSpeed * 6f;
    
        _mc.ApplyForce(dodgeDirection, dodgeForce);
    
        yield return new WaitForSeconds(dodgeDuration);
        isDodging = false;
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
    #endregion

    #region Combat

    public int GetLevel() => 1;// tempPlayerAttributes.level;

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
        /*
        tempPlayerAttributes.ModifyHealth(-incomingDamage);
        Debug.Log("Player takes damage" + tempPlayerAttributes.GetFloatAttribute(TempPlayerStats.health));

        if (tempPlayerAttributes.GetFloatAttribute(TempPlayerStats.health) == 0)
            Die();*/
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
    #endregion

    #region Movement
    private void Movement()
    {
        moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        Vector3 camForward = Camera.main.transform.forward.normalized;
        Vector3 camRight = Camera.main.transform.right.normalized;
        camForward.y = 0;
        camRight.y = 0;
        
        //_movementController.SetTransforms(camForward, camRight);
        
        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        
        //_currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        if (moveDirection.sqrMagnitude < _threshold)
        {
            _animationBlend = 0f;
        }
        else
        {
            _animationBlend = Input.GetKey(KeyCode.LeftShift) ? 6 : 2.5f;
        }
        
        /*
        _animationBlend = Mathf.Lerp(_animationBlend, _currentSpeed, Time.deltaTime * sprintTransitSpeed);
        if (_animationBlend < 0.01f) _animationBlend = 0f;*/
        
        //_movementController.SetMoveDirection(moveDirection, _currentSpeed);

        if (_IsGrounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }
        }
        else
        {
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }
        }
        
        /*
        if (moveDirection.magnitude >= _threshold)
            _movementController.SetMoveDirection(moveDirection, _currentSpeed);
        else
            _movementController.SetMoveDirection(Vector3.zero, 0);*/
        
        if (_hasAnimator)
        {
            float inputMagnitude = 1;
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
        
        //GroundMovement();
        //Turn();
    }

    #region Camera
    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }
    #endregion

    private void GroundMovement()
    {/*
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

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * sprintTransitSpeed);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        move *= speed;

        move.y = VerticalForceCalculation();

        //characterController.Move(move * Time.deltaTime);

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }*/
    }

    private void Turn()
    {/*
        if (Mathf.Abs(turnInput) > 0 || Mathf.Abs(moveInput) > 0)
        {
            Vector3 currentLookDirection = mainCamera.forward;
            currentLookDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(currentLookDirection);

            // Smoothly rotates the player
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turningSpeed);

        }*/
    }
    #endregion

    #region Jump
    private void Jump()
    {
        if (_IsGrounded)
        {
            //_movementController.JumpSqrt();
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, true);
            }
        }
        else
        {
            _input.jump = false;
        }
    }
    #endregion

    #region Dash/Dodge
    private IEnumerator Dodge()
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        float startTime = Time.time;
        //dodgeDirection = -transform.forward;

        while (Time.time < startTime + dodgeDuration)
        {
            //characterController.Move(dodgeDirection * dodgeSpeed * Time.deltaTime);
            yield return null;
        }

        isDodging = false;
    }
    #endregion

    #region Helpers
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    #endregion

    #region Debugging
    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (_IsGrounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;
        
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }
    #endregion

    #region AnimationEvents
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = UnityEngine.Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_mc.transform.position), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_mc.transform.position), FootstepAudioVolume);
        }
    }
    #endregion
}