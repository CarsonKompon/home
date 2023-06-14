using Sandbox;

namespace Home;

public class ChairController : HomePawnController
{
    public ChairBase Chair { get; set; }
    public override bool HasAnimations => false;

    public override void Simulate()
    {
        base.Simulate();

        if(Chair == null) return;

        WishVelocity = Vector3.Zero;
        Velocity = Vector3.Zero;

        var attachment = Chair.GetAttachment("Seat");
        
        if(Pawn is HomePlayer player)
        {
            Rotation rotation;

            // If we're a bot, spin us around 180 degrees
            if ( Client.IsBot )
                rotation = player.ViewAngles.WithYaw( player.ViewAngles.yaw + 180f ).ToRotation();
            else
                rotation = player.ViewAngles.ToRotation();

            var idealRotation = Rotation.LookAt( Vector3.Forward.WithZ( 0 ), Vector3.Up );
            Rotation = Rotation.Slerp( Rotation, idealRotation, Time.Delta * 0.02f );
            Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

            CitizenAnimationHelper animHelper = new CitizenAnimationHelper( player );

            animHelper.WithWishVelocity(WishVelocity);
            animHelper.WithVelocity(Velocity);
            animHelper.WithLookAt( player.EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
            animHelper.AimAngle = rotation;
            
            player.SetAnimParameter( "sit", 1 );
        }

        if(Input.Pressed("crouch"))
        {
            if(Pawn is HomePlayer ply) ply.SetAnimParameter( "sit", 0 );
            if(attachment != null) Position = attachment.Value.Position;
            Position += Chair.ExitOffset.Position;
            Rotation += Chair.ExitOffset.Rotation;
            Chair.RemoveUser();
        }
    }



}