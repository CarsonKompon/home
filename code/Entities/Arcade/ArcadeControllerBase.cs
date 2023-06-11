using Sandbox;

namespace Home;

public class ArcadeControllerBase : HomePawnController
{
    public ArcadeMachineBase ArcadeMachine { get; set; }

    public override void Simulate()
    {
        base.Simulate();

        if(ArcadeMachine == null) return;

        WishVelocity = Vector3.Zero;
        Velocity = Vector3.Zero;
        Position = ArcadeMachine.Position + Vector3.Down * 10 + ArcadeMachine.Rotation.Backward * 50f + Vector3.Up * 10f;
    
        if(Input.Pressed("crouch"))
        {
            ArcadeMachine.RemoveUser();
        }
    }



}