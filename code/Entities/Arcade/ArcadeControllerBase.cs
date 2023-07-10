using Sandbox;

namespace Home;

public partial class ArcadeControllerBase : HomePawnController
{
    public ArcadeMachineBase ArcadeMachine { get; set; }
    public virtual bool AnimateHandsToJoystick => true; 

    public override void Simulate()
    {
        base.Simulate();

        if(!ArcadeMachine.IsValid())
        {
            if(Pawn is HomePlayer ply)
            {
                ply.ResetController();
            }
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
    
        if(Pawn is HomePlayer player)
        {
            if(AnimateHandsToJoystick)
            {
                if(player.GetAnimParameterBool("b_vr") == false) SetVRIK(player, true);

                var leftHandLocal = player.Transform.ToLocal( ArcadeMachine.GetAttachment("hand_L") ?? Transform.Zero );
                var rightHandLocal = player.Transform.ToLocal( ArcadeMachine.GetAttachment("hand_R") ?? Transform.Zero );

                var handOffset = Vector3.Zero;
                player.SetAnimParameter( "left_hand_ik.position", leftHandLocal.Position);
                player.SetAnimParameter( "right_hand_ik.position", rightHandLocal.Position);

                player.SetAnimParameter( "left_hand_ik.rotation", leftHandLocal.Rotation * Rotation.From( 0, 0, 180 ) );
                player.SetAnimParameter( "right_hand_ik.rotation", rightHandLocal.Rotation );

                player.SetAnimParameter( "duck", 0f );
            }

            if(Input.Pressed("crouch"))
            {
                ArcadeMachine.EndGame(Pawn.Client.SteamId);
            }
        }

        BuildInput();
    }

    public virtual void OnExit()
    {
        if(Pawn is not HomePlayer player) return;
        SetVRIK(player, false);
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