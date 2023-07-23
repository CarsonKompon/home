using Sandbox;

namespace Home;

public partial class ArcadeControllerBase : PawnController
{
    [Net] public ArcadeMachineBase ArcadeMachine { get; set; }
    protected CitizenAnimationHelper AnimHelper;
    public override bool HasAnimations => false; // Disable base animator
    public virtual bool AnimateHandsToJoystick => true;
    public Transform PlayerTransform;

    protected override void OnActivate()
    {
        base.OnActivate();
        AnimHelper = new CitizenAnimationHelper(Entity);
        var attachment = ArcadeMachine.GetAttachment("PlayerPos");
        if(attachment != null)
        {
            PlayerTransform = attachment.Value;
        }
        else
        {
            PlayerTransform = new Transform(ArcadeMachine.Position + Vector3.Down * 10 + ArcadeMachine.Rotation.Forward * 40f + Vector3.Up * 10f);
        }
    }

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
        Position = PlayerTransform.Position;
    
        SimulateAnimations();

        if(Input.Pressed("crouch"))
        {
            ArcadeMachine.EndGame(Entity.Client.SteamId);
        }

        BuildInput();
    }

    protected virtual void SimulateAnimations()
    {
        // var turnSpeed = 0.02f;
		// var rotation = Entity.ViewAngles.ToRotation();
		// var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		// Entity.Rotation = Rotation.Slerp( Entity.Rotation, idealRotation, 1f * Time.Delta * turnSpeed );
		// Entity.Rotation = Entity.Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

        // Stay still and grounded, with the ability to look around
        AnimHelper.WithWishVelocity(WishVelocity);
        AnimHelper.WithVelocity(Velocity);
        //AnimHelper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
        //AnimHelper.AimAngle = rotation;
        AnimHelper.IsGrounded = true;
        AnimHelper.DuckLevel = 0;
        AnimHelper.AimBodyWeight = 0.5f;

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
        }
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