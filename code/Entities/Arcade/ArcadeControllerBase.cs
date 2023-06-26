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
        Position = ArcadeMachine.Position + Vector3.Down * 10 + ArcadeMachine.Rotation.Forward * 40f + Vector3.Up * 10f;
    
        if(Input.Pressed("crouch"))
        {
            ArcadeMachine.RemoveUser();
        }

        BuildInput();
    }

    // public override void BuildInput()
    // {
    //     base.BuildInput();

    //     if(Pawn is HomePlayer player)
    //     {
    //         player.WorldInput.Ray = player.AimRay;
    //         player.WorldInput.MouseLeftPressed = Input.Down( "click" );
    //         player.WorldInput.MouseRightPressed = Input.Down( "rightclick" );
    //     }
    // }



}