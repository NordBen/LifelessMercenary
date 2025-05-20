using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LM
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        // movement
        public float movementSpeed = 7f;
        public float airControlRate = 2f;
        public float jumpSpeed = 10f;
        public float jumpDuration = 0.2f;
        public float airFriction = 0.5f;
        public float groundFriction = 100f;
        public float gravity = 30f;
        public float slideGravity = 5f;
        public float slopeLimit = 30f;
        public bool useLocalMomentum;
        
        bool jumpKeyIsPressed;    // Tracks whether the jump key is currently being held down by the player
        bool jumpKeyWasPressed;   // Indicates if the jump key was pressed since the last reset, used to detect jump initiation
        bool jumpKeyWasLetGo;     // Indicates if the jump key was released since it was last pressed, used to detect when to stop jumping
        bool jumpInputIsLocked;   // Prevents jump initiation when true, used to ensure only one jump action per press
        
        private CountdownTimer jumpTimer;
        Vector3 momentum, savedVelocity, savedMovementVelocity;
        
        public event Action<Vector3> OnJump = delegate { };
        public event Action<Vector3> OnLand = delegate { };

        bool IsGrounded2() => IsGrounded();//stateMachine.CurrentState is GroundedState or SlidingState;
        public Vector3 GetVelocity() => savedVelocity;
        public Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
        public Vector3 GetMovementVelocity() => savedMovementVelocity;
        
        // stepheight
        [Header("Collider Settings:")]
        [Range(0f, 1f)] [SerializeField] private float stepHeightRatio = 0.1f;
        [SerializeField] private float colliderHeight = 2f;
        [SerializeField] private float colliderThickness = 1f;
        [SerializeField] private Vector3 colliderOffset = Vector3.zero;
        
        private Rigidbody rb;
        private Transform tr;
        private CapsuleCollider col;
        private GroundCast sensor;
        private CeilingDetector ceilingDetector;
        
        public InteractableActor currentInteractable;
        
        // step height
        private bool isGrounded;
        private float baseSensorRange;
        private Vector3 currentGroundAdjustmentVelocity; // Velocity to adjust player position to maintain ground contact
        private int currentLayer;
        
        [Header("Sensor Settings:")]
        [SerializeField] bool isInDebugMode;
        private bool isUsingExtendedSensorRange = true;
        
        private Camera mainCamera;
        private Vector2 moveInput;
        private PlayerInput playerInput;

        private bool IsJumping = false;

        private void Awake()
        {
            Setup();
            RecalculateColliderDimensions();
        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                RecalculateColliderDimensions();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            playerInput = GetComponent<PlayerInput>();

            //playerInput.onActionTriggered += HandleInputAction;
        }

        private void Update()
        {
            //if (Input.GetKey(KeyCode.Space))
              //  Jump();
              CheckForInteractable();
              if (currentInteractable != null)
              {
                  if (Input.GetKeyDown(KeyCode.E))
                  {
                      CallInteract();
                  }
              }
        }

        private void FixedUpdate()
        {
            CheckForGround();
            HandleMomentum();
            Vector3 velocity = IsGrounded2() ? CalculateMovementVelocity() : Vector3.zero;
            velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
            
            SetExtendSensorRange(IsGrounded2());
            SetVelocity(velocity);

            savedVelocity = velocity;
            savedMovementVelocity = CalculateMovementVelocity();

            ResetJumpKeys();
            
            if (ceilingDetector != null) ceilingDetector.Reset();
        }

        private void LateUpdate()
        {
            if (isInDebugMode)
                sensor.DrawDebug();
        }

        private void HandleInputAction(InputAction.CallbackContext context)
        {
            switch (context.action.name)
            {
                case "Move":
                    moveInput = context.ReadValue<Vector2>();
                    break;
                case "Jump":
                    if (context.performed)
                    {
                        //Jump();
                    }
                    break;
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        public void Jump(InputAction.CallbackContext context)
        {
            IsJumping = true;
            OnJumpStart();
        }
        
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

        private void CallInteract()
        {
            if (currentInteractable.pressToInteract)
            {
                currentInteractable.Interact();
            }
            else
                Debug.Log("touch object instead");
        }
        
        #region Movement
        Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;

        Vector3 CalculateMovementDirection() {
            Vector3 direction = mainCamera == null 
                ? tr.right * moveInput.x + tr.forward * moveInput.y 
                : Vector3.ProjectOnPlane(mainCamera.transform.right, tr.up).normalized * moveInput.x + 
                  Vector3.ProjectOnPlane(mainCamera.transform.forward, tr.up).normalized * moveInput.y;
            
            return direction.magnitude > 1f ? direction.normalized : direction;
        }

        void HandleMomentum() {
            if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
            
            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;
            
            verticalMomentum -= tr.up * (gravity * Time.deltaTime);
            if (IsGrounded2() && VectorMath.GetDotProduct(verticalMomentum, tr.up) < 0f) {
                verticalMomentum = Vector3.zero;
            }

            if (!IsGrounded()) {
                AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
            }

            /* // sliding
            if (stateMachine.CurrentState is SlidingState) {
                HandleSliding(ref horizontalMomentum);
            }*/

            float friction = IsGrounded2() ? groundFriction : airFriction; //stateMachine.CurrentState is GroundedState ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            
            momentum = horizontalMomentum + verticalMomentum;

            if (IsJumping)//stateMachine.CurrentState is JumpingState)
            {
                HandleJumping();
            }
            /*
            if (sliding) {
                momentum = Vector3.ProjectOnPlane(momentum, GetGroundNormal());
                if (VectorMath.GetDotProduct(momentum, tr.up) > 0f) {
                    momentum = VectorMath.RemoveDotVector(momentum, tr.up);
                }
            
                Vector3 slideDirection = Vector3.ProjectOnPlane(-tr.up, GetGroundNormal()).normalized;
                momentum += slideDirection * (slideGravity * Time.deltaTime);
            }*/
            
            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
        }
        
        void HandleJumpKeyInput(bool isButtonPressed) {
            if (!jumpKeyIsPressed && isButtonPressed) {
                jumpKeyWasPressed = true;
            }

            if (jumpKeyIsPressed && !isButtonPressed) {
                jumpKeyWasLetGo = true;
                jumpInputIsLocked = false;
            }
            
            jumpKeyIsPressed = isButtonPressed;
        }

        void HandleJumping() {
            momentum = VectorMath.RemoveDotVector(momentum, tr.up);
            momentum += tr.up * jumpSpeed;
        }

        void ResetJumpKeys() {
            jumpKeyWasLetGo = false;
            jumpKeyWasPressed = false;
        }

        public void OnJumpStart() {
            if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
            
            momentum += tr.up * jumpSpeed;
            jumpTimer.Start();
            jumpInputIsLocked = true;
            OnJump.Invoke(momentum);
            
            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
        }

        public void OnGroundContactLost() {
            if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
            
            Vector3 velocity = GetMovementVelocity();
            if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f) {
                Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);
                float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);
                
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) velocity = Vector3.zero;
                else if (dot > 0f) velocity -= projectedMomentum;
            }
            momentum += velocity;
            
            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
        }

        public void OnGroundContactRegained() {
            Vector3 collisionVelocity = useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
            OnLand.Invoke(collisionVelocity);
        }

        public void OnFallStart() {
            var currentUpMomemtum = VectorMath.ExtractDotVector(momentum, tr.up);
            momentum = VectorMath.RemoveDotVector(momentum, tr.up);
            momentum -= tr.up * currentUpMomemtum.magnitude;
        }
        
        void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity) {
            if (horizontalMomentum.magnitude > movementSpeed) {
                if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f) {
                    movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                }
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * 0.25f);
            }
            else {
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
            }
        }

        void HandleSliding(ref Vector3 horizontalMomentum) {
            Vector3 pointDownVector = Vector3.ProjectOnPlane(GetGroundNormal(), tr.up).normalized;
            Vector3 movementVelocity = CalculateMovementVelocity();
            movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }
        #endregion

        #region Collider/StepheightStuff
        public void CheckForGround() {
            if (currentLayer != gameObject.layer) {
                RecalculateSensorLayerMask();
            }
            
            currentGroundAdjustmentVelocity = Vector3.zero;
            sensor.castLength = isUsingExtendedSensorRange 
                ? baseSensorRange + colliderHeight * tr.localScale.x * stepHeightRatio
                : baseSensorRange;
            sensor.Cast();
            
            isGrounded = sensor.HasDetectedHit();
            if (!isGrounded) return;
            
            float distance = sensor.GetDistance();
            float upperLimit = colliderHeight * tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
            float middle = upperLimit + colliderHeight * tr.localScale.x * stepHeightRatio;
            float distanceToGo = middle - distance;
            
            currentGroundAdjustmentVelocity = tr.up * (distanceToGo / Time.fixedDeltaTime);
        }
        
        public bool IsGrounded() => isGrounded;
        public Vector3 GetGroundNormal() => sensor.GetNormal();
        
        public void SetVelocity(Vector3 velocity) => rb.linearVelocity = velocity + currentGroundAdjustmentVelocity;
        public void SetExtendSensorRange(bool isExtended) => isUsingExtendedSensorRange = isExtended;

        void Setup() {
            tr = transform;
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            ceilingDetector = GetComponent<CeilingDetector>();

            jumpTimer = new CountdownTimer(jumpDuration);
            
            rb.freezeRotation = true;
            rb.useGravity = false;
        }

        void RecalculateColliderDimensions() {
            if (col == null) {
                Setup();
            }
            
            col.height = colliderHeight * (1f - stepHeightRatio);
            col.radius = colliderThickness / 2f;
            col.center = colliderOffset * colliderHeight + new Vector3(0f, stepHeightRatio * col.height / 2f, 0f);

            if (col.height / 2f < col.radius) {
                col.radius = col.height / 2f;
            }
            
            RecalibrateSensor();
        }

        void RecalibrateSensor() {
            sensor ??= new GroundCast(tr);
            
            sensor.SetCastOrigin(col.bounds.center);
            sensor.SetCastDirection(GroundCast.CastDirection.Down);
            RecalculateSensorLayerMask();
            
            const float safetyDistanceFactor = 0.001f; // Small factor added to prevent clipping issues when the sensor range is calculated
            
            float length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
            baseSensorRange = length * (1f + safetyDistanceFactor) * tr.localScale.x;
            sensor.castLength = length * tr.localScale.x;
        }

        void RecalculateSensorLayerMask() {
            int objectLayer = gameObject.layer;
            int layerMask = Physics.AllLayers;

            for (int i = 0; i < 32; i++) {
                if (Physics.GetIgnoreLayerCollision(objectLayer, i)) {
                    layerMask &= ~(1 << i);
                }
            }
            
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);
            
            sensor.layermask = layerMask;
            currentLayer = objectLayer;
        }
        #endregion
    }
}