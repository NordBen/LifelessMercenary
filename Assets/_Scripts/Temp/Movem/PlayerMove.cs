using UnityEngine;

public class PlayerMove : MonoBehaviour
{
}

public interface IStateS2
{
    void Update() { }
    void FixedUpdate() { }
    void OnEnter() { }
    void OnExit() { }
}

public class GroundedState : IStateS2
{
    readonly TempMovementController controller;

    public GroundedState(TempMovementController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactRegained();
    }
}

public class FallingState : IStateS2
{
    readonly TempMovementController controller;

    public FallingState(TempMovementController controller)
    {
        this.controller = controller;
    }

    public void OnEnter() 
    { 
        controller.OnFallStart();
    }
}

public class RisingState : IStateS2
{
    readonly TempMovementController controller;

    public RisingState(TempMovementController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
    }
}

public class JumpingState : IStateS2
{
    readonly TempMovementController controller;

    public JumpingState(TempMovementController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
        controller.OnJumpStart();
    }
}

public class SlidingState : IStateS2
{
    readonly TempMovementController controller;

    public SlidingState(TempMovementController controller)
    {
        this.controller = controller;
    }

    public void OnEnter()
    {
        controller.OnGroundContactLost();
    }
}