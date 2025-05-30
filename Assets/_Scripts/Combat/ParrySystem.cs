using UnityEngine;

public class ParrySystem : MonoBehaviour
{
    public Animator animator;
    private bool parryWindowActive = false;
    private bool canParry = true;

    void Update()
    {
        if (Input.GetKey(KeyCode.F) && canParry)
        {
            animator.SetTrigger("tParry");
            canParry = false; // This is to prevent spammig
        }
    }
    // This is also called by an animation event
    public void DisableParryWindow()
    {
        parryWindowActive = false;
        canParry = true; // This resets so the player can parry again
        Debug.Log("You should now no longer be able to parry :)");
    }

    // This is called by the animation event
    public void EnableParryWindow()
    {
        parryWindowActive = true;
        Debug.Log("The parry window should be active now :)");
    }


    public bool TryParryAttack()
    {
        if (parryWindowActive)
        {
            Debug.Log("Parry is afoot");
            return true;
        }
        else
        {
            Debug.Log("Parry not afoot");
            return false;
        }
    }
}
