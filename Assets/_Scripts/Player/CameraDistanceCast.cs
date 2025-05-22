using UnityEngine;

namespace LM
{
    public class CameraDistanceCast : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform targetTransform;
        public LayerMask layerMask = Physics.AllLayers;
        public float minimumDistanceFromObstacle = 0.1f;
        public float smoothingFactor = 25f;
        [SerializeField] private float distance;

        private Transform tr;
        private float currentDistance;

        private void Awake()
        {
            tr = transform;
            layerMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
            currentDistance = (targetTransform.position - cameraTransform.position).magnitude;
            ;
        }

        private void LateUpdate()
        {
            Vector3 castDirection = targetTransform.position - tr.position;

            float apDistance = GetCameraDistance(castDirection);
            currentDistance = Mathf.Lerp(currentDistance, apDistance, Time.deltaTime * smoothingFactor);
            cameraTransform.position = tr.position + castDirection.normalized * currentDistance;
        }

        private float GetCameraDistance(Vector3 castDirection)
        {
            float distance = castDirection.magnitude + minimumDistanceFromObstacle;
            /*if (Physics.Raycast(new Ray(tr.position, castDirection), out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacle);
            }*/
            float sphereRadius = 0.5f;
            if (Physics.SphereCast(new Ray(tr.position, castDirection), sphereRadius, out RaycastHit hit, distance, layerMask, QueryTriggerInteraction.Ignore))
            {
                return Mathf.Max(0f, hit.distance - minimumDistanceFromObstacle);
            }
            return castDirection.magnitude;
        }
    }
}