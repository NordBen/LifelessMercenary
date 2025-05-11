using UnityEngine;

public class CeilingDetector : MonoBehaviour
{
    public float ceilingAngleLimit = 10f;
    public bool useDebug;
    private float debugDrawDuration = 0.2f;
    private bool ceilingWasHit;

    private void OnCollisionEnter(Collision collision) => CheckForContact(collision);
    private void OnCollisionStay(Collision collision) => CheckForContact(collision);

    private void CheckForContact(Collision collision)
    {
        if (collision.contacts.Length == 0) return;

        float angle = Vector3.Angle(-this.transform.up, collision.contacts[0].normal);

        if (angle < ceilingAngleLimit) ceilingWasHit = true;

        if (useDebug) Debug.DrawRay(collision.contacts[0].point, collision.contacts[0].normal, Color.red, debugDrawDuration);
    }

    public bool HitCeiling() => ceilingWasHit;
    public void Reset() => ceilingWasHit = false;
}