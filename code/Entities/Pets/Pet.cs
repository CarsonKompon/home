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

    protected NavPath? Path;

    protected int CurrentPathSegment;
    protected TimeSince TimeSinceGeneratedPath = 0;

    public virtual float MovementSpeed => 2f;
    public virtual float FollowDistance => 48f;
    public Vector3 PreviousVelocity = Vector3.Zero;

    public Pet()
    {
        //Unstuck = new HomeUnstuck(this);
    }

    public override void Spawn()
    {
        base.Spawn();

        Tags.Add("npc");
        Tags.Remove("solid");
    } 

    [GameEvent.Tick.Server]
    protected void ServerTick()
    {
        if(!Player.IsValid())
        {
            Delete();
            return;
        }
        
        StateTick();
    }

    protected virtual void StateTick()
    {
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

    protected virtual void GeneratePath()
    {
        TimeSinceGeneratedPath = 0;

        Path = NavMesh.PathBuilder(Position)
                .WithMaxClimbDistance(16f)
                .WithMaxDropDistance(16f)
                .WithStepHeight(16f)
                .WithMaxDistance(99999999)
                .WithPartialPaths()
                .Build(Player.Position);

        CurrentPathSegment = 0;
    }

    protected virtual void TraversePath()
    {
        if(Path == null) return;

        var distanceToTravel = MovementSpeed;

        while(distanceToTravel > 0)
        {
            var currentTarget = Path.Segments[CurrentPathSegment];
            var distanceToTarget = Position.Distance(currentTarget.Position);

            if(distanceToTarget > distanceToTravel)
            {
                var direction = (currentTarget.Position - Position).Normal;
                PreviousVelocity = direction * distanceToTravel;
                Position += PreviousVelocity;
                return;
            }
            else
            {
                var direction = (currentTarget.Position - Position).Normal;
                PreviousVelocity = direction * distanceToTarget;
                Position += PreviousVelocity;
                distanceToTravel -= distanceToTarget;
                CurrentPathSegment++;
            }

            if(CurrentPathSegment == Path.Count)
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

    // protected void GetUnstuck()
    // {
    //     public virtual bool TestAndFix()
	// 	{
	// 		var result = Controller.TraceBBox( Controller.Position, Controller.Position );

	// 		// Not stuck, we cool
	// 		if ( !result.StartedSolid )
	// 		{
	// 			StuckTries = 0;
	// 			return false;
	// 		}

	// 		if ( result.StartedSolid )
	// 		{
	// 			if ( HomeBasePlayerController.Debug )
	// 			{
	// 				DebugOverlay.Text( $"[stuck in {result.Entity}]", Controller.Position, Color.Red );
	// 				DebugOverlay.Box( result.Entity, Color.Red );
	// 			}
	// 		}

	// 		//
	// 		// Client can't jiggle its way out, needs to wait for
	// 		// server correction to come
	// 		//
	// 		if ( Game.IsClient )
	// 			return true;

	// 		int AttemptsPerTick = 20;

	// 		for ( int i=0; i< AttemptsPerTick; i++ )
	// 		{
	// 			var pos = Controller.Position + Vector3.Random.Normal * (((float)StuckTries) / 2.0f);

	// 			// First try the up direction for moving platforms
	// 			if ( i == 0 )
	// 			{
	// 				pos = Controller.Position + Vector3.Up * 5;
	// 			}

	// 			result = Controller.TraceBBox( pos, pos );

	// 			if ( !result.StartedSolid )
	// 			{
	// 				if ( HomeBasePlayerController.Debug )
	// 				{
	// 					DebugOverlay.Text( $"unstuck after {StuckTries} tries ({StuckTries* AttemptsPerTick} tests)", Controller.Position, Color.Green, 5.0f );
	// 					DebugOverlay.Line( pos, Controller.Position, Color.Green, 5.0f, false );
	// 				}

	// 				Controller.Position = pos;
	// 				return false;
	// 			}
	// 			else
	// 			{
	// 				if ( HomeBasePlayerController.Debug )
	// 				{
	// 					DebugOverlay.Line( pos, Controller.Position, Color.Yellow, 0.5f, false );
	// 				}
	// 			}
	// 		}

	// 		StuckTries++;

	// 		return true;
	// 	}
    // }

}