using UnityEngine;

public class LockCursor : MonoBehaviour
{
    public bool cursorLocked = true;

    private void Start()
    {
        SetCursorState(cursorLocked);
    }

    private void OnApplicationFocus(bool focus)
    {
        SetCursorState(cursorLocked);
    }

    public void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
