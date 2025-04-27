using UnityEngine;

public class Ragdoll : MonoBehaviour
{
    Rigidbody[] rigidbodies;
    Animator animator;
    
    void Start()
    {
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        animator = GetComponent<Animator>();
        DeactivateRagdoll();
    }
    
    public void DeactivateRagdoll()
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
        }
        animator.enabled = true;
    }
    
    public void ActivateRagdoll()
    {
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = false;
        }
        animator.enabled = false;
    }
    
    public void ApplyForce(Vector3 force)
    {
        var rb = animator.GetBoneTransform(HumanBodyBones.Hips).GetComponent<Rigidbody>();
        rb.AddForce(force, ForceMode.VelocityChange);
    }
}
