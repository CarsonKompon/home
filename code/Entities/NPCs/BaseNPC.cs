using Sandbox;
using Editor;
using System.Collections.Generic;
using System.Linq;

namespace Home;

/// <summary>
/// Talking to this NPC will allow players to check-in to an available room.
/// </summary>
public partial class BaseNPC : AnimatedEntity, IUse
{
    protected Transform StartingTransform = new();
    protected string ClothingString = "";
    ClothingContainer Clothing = new();

    public virtual bool IsUsable( Entity user ) => true;

    public override void Spawn()
    {
        base.Spawn();

        StartingTransform = Transform;

        // Setup physics
        SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );
        EnableHitboxes = true;

        // Set the model and dress it
        SetModel("models/citizen/citizen.vmdl");
        Clothing.Deserialize(ClothingString);
        Clothing.DressEntity( this );
    }

    [GameEvent.Tick.Server]
	void Tick()
	{
        // Initialize some variables
        Vector3 targetPos = Position + Rotation.Forward * 100;
        Rotation targetRot = Rotation;

        // Find the nearest player
        IEnumerable<HomePlayer> nearestPlayers = HomePlayer.FindInSphere(Position, 200f).OfType<HomePlayer>();
        if(nearestPlayers.Count() > 0)
        {
            HomePlayer nearestPlayer = nearestPlayers.FirstOrDefault<HomePlayer>();
            targetPos = nearestPlayer.GetBoneTransform( nearestPlayer.GetBoneIndex( "head" ) ).Position + Vector3.Down * 50;
            targetRot = Rotation.LookAt( targetPos - Position );
        }

        CitizenAnimationHelper animHelper = new CitizenAnimationHelper(this);

        animHelper.WithWishVelocity( Velocity);
        animHelper.WithVelocity( Velocity );
        animHelper.WithLookAt( targetPos, 0.75f, 0.5f, 0.25f);
        animHelper.AimAngle = targetRot;
	}

    public virtual bool OnUse(Entity user)
    {
        return false;
    } 

}