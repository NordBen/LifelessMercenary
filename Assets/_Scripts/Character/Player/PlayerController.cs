using System.Xml.Serialization;
using Unity.Mathematics;
using UnityEditor.Search;
using UnityEngine;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;


public class PlayerController : MonoBehaviour, ICombat
{
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

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

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

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

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float sprintTransitSpeed = 5f; // How fast the caracter transitions to sprint
    [SerializeField] private float turningSpeed = 10f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpHeight = 2f;
    private float _animationBlend;

    private float verticalVelocity;
    private float speed;

    private float _fallTimeoutDelta;

    [Header("Input")]
    private float moveInput;
    private float turnInput;

    private PlayerInput _playerInput;
    private Animator _animator;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;
    private bool _IsGrounded = true;

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
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _input = GetComponent<StarterAssetsInputs>();
        _playerInput = GetComponent<PlayerInput>();

        AssignAnimationIDs();

        characterController = GetComponent<CharacterController>();
        tempPlayerAttributes = GameObject.Find("PlayerStats").GetComponent<TempPlayerAttributes>();

        _fallTimeoutDelta = FallTimeout;
    }

    #region Update
    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);

        _IsGrounded = characterController.isGrounded;
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, _IsGrounded);
        }

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
    }

    private void LateUpdate()
    {
        CameraRotation();
    }
    #endregion
    #endregion

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void InputManagement()
    {
        // Get the input from our keyboard
        moveInput = Input.GetAxis("Horizontal");
        turnInput = Input.GetAxis("Vertical");
    }

    #region Interaction
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
    #endregion

    #region Combat
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
    #endregion

    #region Movement
    private void Movement()
    {
        GroundMovement();
        Turn();
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

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * sprintTransitSpeed);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        move *= speed;

        move.y = VerticalForceCalculation();

        characterController.Move(move * Time.deltaTime);

        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }
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
    #endregion

    #region Jump
    private float VerticalForceCalculation()
    {
        if (_IsGrounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            verticalVelocity = -1;

            if (Input.GetButtonDown("Jump"))
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * gravity * 2);
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            _input.jump = false;
        }
        return verticalVelocity;
    }
    #endregion

    #region Dash/Dodge
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

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
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
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(characterController.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(characterController.center), FootstepAudioVolume);
        }
    }
    #endregion
}