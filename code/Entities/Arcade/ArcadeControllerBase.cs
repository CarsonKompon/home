using Sandbox;

namespace Home;

public partial class ArcadeControllerBase : PawnController
{
    [Net] public ArcadeMachineBase ArcadeMachine { get; set; }
    public override bool HasAnimations => false; // Disable base animator
    public virtual bool AnimateHandsToJoystick => true; 

    public override void Simulate()
    {
        base.Simulate();

        if(Game.IsServer && !ArcadeMachine.IsValid())
        {
            Entity.ResetController();
            return;
        }

        WishVelocity = Vector3.Zero;
        Velocity = Vector3.Zero;

        var attachment = ArcadeMachine.GetAttachment("PlayerPos");
        if(attachment != null)
        {
            Position = attachment.Value.Position;
        }
        else
        {
            Position = ArcadeMachine.Position + Vector3.Down * 10 + ArcadeMachine.Rotation.Forward * 40f + Vector3.Up * 10f;
        }
    
        if(AnimateHandsToJoystick)
        {
            if(Entity.GetAnimParameterBool("b_vr") == false) SetVRIK(Entity, true);

            var leftHandLocal = Entity.Transform.ToLocal( ArcadeMachine.GetAttachment("hand_L") ?? Transform.Zero );
            var rightHandLocal = Entity.Transform.ToLocal( ArcadeMachine.GetAttachment("hand_R") ?? Transform.Zero );

            var handOffset = Vector3.Zero;
            Entity.SetAnimParameter( "left_hand_ik.position", leftHandLocal.Position);
            Entity.SetAnimParameter( "right_hand_ik.position", rightHandLocal.Position);

            Entity.SetAnimParameter( "left_hand_ik.rotation", leftHandLocal.Rotation * Rotation.From( 0, 0, 180 ) );
            Entity.SetAnimParameter( "right_hand_ik.rotation", rightHandLocal.Rotation );

            Entity.SetAnimParameter( "duck", 0f );
        }

        Entity.SetAnimParameter( "b_grounded", true );

        if(Input.Pressed("crouch"))
        {
            ArcadeMachine.EndGame(Entity.Client.SteamId);
        }

        BuildInput();
    }

    public virtual void OnExit()
    {
        SetVRIK(Entity, false);
    }

    public void SetVRIK(HomePlayer player, bool enabled)
    {
        player.SetAnimParameter("b_vr", enabled);
        SetVRIKRPC(player, enabled);
    }

    [ClientRpc]
    public static void SetVRIKRPC(HomePlayer player, bool enabled)
    {
        player.SetAnimParameter("b_vr", enabled);
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