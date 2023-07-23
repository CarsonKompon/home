using Sandbox;

namespace Home;

public class ChairController : PawnController
{
    public ChairBase Chair { get; set; }
    public override bool HasAnimations => false;

    public override void Simulate()
    {
        base.Simulate();

        if(!Chair.IsValid())
        {
            Entity.ResetController();
            return;
        }

        WishVelocity = Vector3.Zero;
        Velocity = Vector3.Zero;

        var attachment = Chair.GetAttachment("Seat");
        
        
        Rotation rotation;

        // If we're a bot, spin us around 180 degrees
        if ( Entity.Client.IsBot )
            rotation = Entity.ViewAngles.WithYaw( Entity.ViewAngles.yaw + 180f ).ToRotation();
        else
            rotation = Entity.ViewAngles.ToRotation();

        var idealRotation = Rotation.LookAt( Vector3.Forward.WithZ( 0 ), Vector3.Up );
        Rotation = Rotation.Slerp( Rotation, idealRotation, Time.Delta * 0.02f );
        Rotation = Rotation.Clamp( idealRotation, 45.0f, out var shuffle ); // lock facing to within 45 degrees of look direction

        CitizenAnimationHelper animHelper = new CitizenAnimationHelper( Entity );

        animHelper.WithWishVelocity(WishVelocity);
        animHelper.WithVelocity(Velocity);
        animHelper.WithLookAt( Entity.EyePosition + EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f );
        animHelper.AimAngle = rotation;
        
        Entity.SetAnimParameter( "sit", 1 );

        if(Input.Pressed("crouch"))
        {
            Entity.SetAnimParameter( "sit", 0 );
            if(attachment != null) Position = attachment.Value.Position;
            Position += Chair.ExitOffset.Position;
            Rotation += Chair.ExitOffset.Rotation;
            Chair.RemoveUser();
        }
    }



}