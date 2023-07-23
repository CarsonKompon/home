using System;
using Sandbox;

namespace Home;

public partial class AnimatorComponent : EntityComponent<HomePlayer>
{
    public void Simulate()
    {
        if(Entity.Controller == null) return;
        if(!Entity.Controller.HasAnimations) return;

		var turnSpeed = 0.02f;
        var controller = Entity.Controller;
		Rotation rotation;

		// If we're a bot, spin us around 180 degrees
		if ( Entity.Client.IsBot )
			rotation = Entity.ViewAngles.WithYaw( Entity.ViewAngles.yaw + 180f ).ToRotation();
		else
			rotation = Entity.ViewAngles.ToRotation();

		var idealRotation = Rotation.LookAt( rotation.Forward.WithZ( 0 ), Vector3.Up );
		Entity.Rotation = Rotation.Slerp( Entity.Rotation, idealRotation, controller.WishVelocity.Length * Time.Delta * turnSpeed );
		Entity.Rotation = Entity.Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

		CitizenAnimationHelper animHelper = new CitizenAnimationHelper( Entity );

		animHelper.WithWishVelocity(controller.WishVelocity);
		animHelper.WithVelocity(controller.Velocity);
		animHelper.WithLookAt( Entity.EyePosition + Entity.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
		animHelper.AimAngle = rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Game.LocalPawn == Entity) ? Voice.Level : Entity.Client.Voice.CurrentLevel;
		animHelper.IsGrounded = Entity.GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsNoclipping = controller.HasTag( "noclip" );
		animHelper.IsClimbing = controller.HasTag( "climbing" );
		animHelper.IsSwimming = Entity.GetWaterLevel() >= 0.5f;
		animHelper.IsWeaponLowered = false;
		animHelper.MoveStyle = Input.Down( "walk" ) ? CitizenAnimationHelper.MoveStyles.Walk : CitizenAnimationHelper.MoveStyles.Run;
	

		if ( controller.HasEvent( "jump" ) ) animHelper.TriggerJump();
		// if ( ActiveChild != lastWeapon ) animHelper.TriggerDeploy();

		if ( Entity.ActiveChild is HomeBaseCarriable carry )
		{
			carry.SimulateAnimator( animHelper );
		}
		else
		{
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}
    }

}