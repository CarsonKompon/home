using System;
using System.Linq;
using Sandbox;
using Home.Util;

namespace Home;

public partial class PetMelon : Pet
{

    public float BaseForce { get; set; } = 15000;
    public float CorrectionForce { get; set; } = 300;

    public override void Spawn()
    {
        base.Spawn();

        InitModel();
    }

    async void InitModel()
    {
        SetModel( await PackageHelper.GetPrimaryAsset("trend.watermelon") );
        // SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

        PhysicsEnabled = true;
        UsePhysicsCollision = true;

        Tags.Add("solid");
    }

    protected override void TraversePath()
    {
        if(Path == null) return;
        if(Path.Count <= 0) return;

        Vector3 targetLocation = Player.Position;
        if(CurrentPathSegment < Path.Count)
        {
            var pathSegment = Path.Segments[CurrentPathSegment];
            targetLocation = pathSegment.Position;
            if(Position.Distance(targetLocation) < 64)
            {
                CurrentPathSegment++;
            }
        }

        MoveTowards(targetLocation);
    }

    protected override void TickAnimation()
    {
        SetAnimParameter( "grounded", true );
        SetAnimParameter( "velocity", PreviousVelocity.Length);

        if(State != PetState.Idle)
        {
            // Set Rotation from PreviousVelocity
            Rotation = Rotation.LookAt( PreviousVelocity.WithZ( 0 ), Vector3.Up );
        }
    }

    protected override void GeneratePath()
    {
        TimeSinceGeneratedPath = 0;

        Path = NavMesh.PathBuilder(Position)
            .WithMaxClimbDistance(4)
            .WithStepHeight(4)
            .WithMaxDistance(99999999)
            .WithPartialPaths()
            .Build(Player.Position);

        CurrentPathSegment = 0;
        TimeSinceGeneratedPath = 0;
    }

    // Thank you Igrium for this code
    public void MoveTowards(Vector3 targetPos)
    {
        targetPos = targetPos.WithZ( Position.z );

        Vector3 normal = (targetPos - Position).Normal;
        Vector2 normal2D = new Vector2(normal.x, normal.y);

        var axis = normal.RotateAround(Vector3.Zero, Rotation.FromYaw(90));

        float torque = BaseForce * .6f;

        PhysicsBody.ApplyTorque(axis * torque);

        // determine angle to account for existing velocity
        Vector2 velocity2D = new Vector2(PhysicsBody.Velocity.x, PhysicsBody.Velocity.y);
        float angle = MeasureAngle(velocity2D, normal2D);

        float correctionMagnitude = velocity2D.Length * MathF.Sin(angle);
        PhysicsBody.ApplyTorque(normal * correctionMagnitude * CorrectionForce);
    }
    

    /// <summary>
    /// Return the angle created by two vectors, given they start at the same point.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private float MeasureAngle(Vector2 a, Vector2 b)
    {
        float angleA = MathF.Atan2(a.y, a.x);
        float angleB = MathF.Atan2(b.y, b.x);
        return angleA - angleB;
    }

}