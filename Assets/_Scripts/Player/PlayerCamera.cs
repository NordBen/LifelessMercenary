using UnityEngine;
using UnityEngine.InputSystem;

namespace LM
{
    public class PlayerCamera : MonoBehaviour
    {
        float currentXAngle;
        float currentYAngle;
        
        [Range(0f, 90f)] public float upperVerticalLimit = 35f;
        [Range(0f, 90f)] public float lowerVerticalLimit = 35f;
        
        public float cameraSpeed = 50f;
        public bool smoothCameraRotation;
        [Range(1f, 50f)] public float cameraSmoothingFactor = 25f;
        
        Transform tr;
        Camera cam;

        private Vector2 mouseLook;
        //[SerializeField, Required] InputReader input;
        
        public Vector3 GetUpDirection() => tr.up;
        public Vector3 GetFacingDirection () => tr.forward;

        void Awake() 
        {
            tr = transform;
            cam = GetComponentInChildren<Camera>();
            
            currentXAngle = tr.localRotation.eulerAngles.x;
            currentYAngle = tr.localRotation.eulerAngles.y;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            mouseLook = context.ReadValue<Vector2>();
        }

        void Update() 
        {
            RotateCamera(mouseLook.x, mouseLook.y);
        }

        void RotateCamera(float horizontalInput, float verticalInput)
        {
            if (smoothCameraRotation) {
                horizontalInput = Mathf.Lerp(0, horizontalInput, Time.deltaTime * cameraSmoothingFactor);
                verticalInput = Mathf.Lerp(0, verticalInput, Time.deltaTime * cameraSmoothingFactor);
            }
            
            currentXAngle += verticalInput * cameraSpeed * Time.deltaTime;
            currentYAngle += horizontalInput * cameraSpeed * Time.deltaTime;
            
            currentXAngle = Mathf.Clamp(currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
            
            tr.localRotation = Quaternion.Euler(currentXAngle, currentYAngle, 0);
        }
    }
}