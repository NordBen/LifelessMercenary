using UnityEngine;

namespace LM
{
    public class TurnWithCamera : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        
        public float turnSpeed = 50f;

        private Transform tr;
        private float currentYRotation;
        private const float fallOffAngle = 90f;

        private void Start()
        {
            tr = transform;

            currentYRotation = tr.localEulerAngles.y;
        }

        private void LateUpdate()
        {
            Vector3 velocity = Vector3.ProjectOnPlane(controller.GetMovementVelocity(), tr.parent.up);
            if (velocity.magnitude < 0.001f) return;
            float angleDifference = VectorMath.GetAngle(tr.forward, velocity.normalized, tr.parent.up);
            
            float step = Mathf.Sign(angleDifference) * Mathf.InverseLerp(0f, fallOffAngle, Mathf.Abs(angleDifference)) * Time.deltaTime * turnSpeed;

            currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDifference) ? angleDifference : step;
            tr.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }
}