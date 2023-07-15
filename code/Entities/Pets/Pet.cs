using System;
using System.Linq;
using Sandbox;

namespace Home;

public partial class Pet : AnimatedEntity
{
    enum PetState { Idle, Following }
    PetState State = PetState.Idle;

    HomePlayer Player;

    protected Vector3[] Path;

    protected int CurrentPathSegment;
    protected TimeSince TimeSinceGeneratedPath = 0;

    public float MovementSpeed => 2f;
    public float FollowDistance => 48f;

    public Pet(HomePlayer player)
    {
        Player = player;
    }

    [GameEvent.Tick.Server]
    public void Tick()
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

    }

    protected void TickIdle()
    {
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
                Position += direction * distanceToTravel;
                return;
            }
            else
            {
                var direction = (currentTarget - Position).Normal;
                Position += direction * distanceToTarget;
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

}