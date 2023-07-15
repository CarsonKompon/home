using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Home;

public partial class Pet : AnimatedEntity
{
    protected enum PetState { Idle, Following }
    protected PetState State = PetState.Idle;

    public HomePlayer Player;

    protected Vector3[] Path;

    protected int CurrentPathSegment;
    protected TimeSince TimeSinceGeneratedPath = 0;

    public float MovementSpeed => 2f;
    public float FollowDistance => 48f;
    public Vector3 PreviousVelocity = Vector3.Zero;
    

    [GameEvent.Tick.Server]
    public void ServerTick()
    {
        if(!Player.IsValid())
        {
            Delete();
            return;
        }
        
        switch( State )
        {
            case PetState.Idle:
                TickIdle();
                break;
            case PetState.Following:
                TickFollowing();
                break;
        }

        TickAnimation();
    }

    protected void TickIdle()
    {
        PreviousVelocity = PreviousVelocity.LerpTo( Vector3.Zero, 0.25f );

        if( Player.Position.Distance(Position) > FollowDistance )
        {
            State = PetState.Following;
        }
    }

    protected void TickFollowing()
    {
        if( Player.Position.Distance(Position) < FollowDistance )
        {
            State = PetState.Idle;
        }

        if( TimeSinceGeneratedPath > 1f )
        {
            GeneratePath();
        }

        TraversePath();
    }

    protected void GeneratePath()
    {
        TimeSinceGeneratedPath = 0;

        Path = NavMesh.PathBuilder(Position)
                .WithMaxClimbDistance(16f)
                .WithMaxDropDistance(16f)
                .WithStepHeight(16f)
                .WithMaxDistance(99999999)
                .WithPartialPaths()
                .Build(Player.Position)
                .Segments
                .Select( x => x.Position )
                .ToArray();

        CurrentPathSegment = 0;
    }

    protected void TraversePath()
    {
        if(Path == null) return;

        var distanceToTravel = MovementSpeed;

        while(distanceToTravel > 0)
        {
            var currentTarget = Path[CurrentPathSegment];
            var distanceToTarget = Position.Distance(currentTarget);

            if(distanceToTarget > distanceToTravel)
            {
                var direction = (currentTarget - Position).Normal;
                PreviousVelocity = direction * distanceToTravel;
                Position += PreviousVelocity;
                return;
            }
            else
            {
                var direction = (currentTarget - Position).Normal;
                PreviousVelocity = direction * distanceToTarget;
                Position += PreviousVelocity;
                distanceToTravel -= distanceToTarget;
                CurrentPathSegment++;
            }

            if(CurrentPathSegment == Path.Count())
            {
                Path = null;
                return;
            }
        }
    }

    protected virtual void TickAnimation()
    {

    }

    public virtual void DressFromString(string clothingString)
    {

    }

}