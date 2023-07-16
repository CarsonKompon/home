using Sandbox;

namespace Home;

public class UnstuckEntity
{
	public ModelEntity Entity;

	public bool IsActive; // replicate

	internal int StuckTries = 0;

	public UnstuckEntity( ModelEntity entity )
	{
		Entity = entity;
	}

	public virtual bool TestAndFix()
	{
		TraceResult result = TraceBBox( Entity.Position, Entity.Position );

		// Not stuck, we cool
		if ( !result.StartedSolid )
		{
			StuckTries = 0;
			return false;
		}

		//
		// Client can't jiggle its way out, needs to wait for
		// server correction to come
		//
		if ( Game.IsClient )
			return true;

		int AttemptsPerTick = 20;

		for ( int i=0; i< AttemptsPerTick; i++ )
		{
			var pos = Entity.Position + Vector3.Random.Normal * (((float)StuckTries) / 2.0f);

			// First try the up direction for moving platforms
			if ( i == 0 )
			{
				pos = Entity.Position + Vector3.Up * 5;
			}

			result = TraceBBox( pos, pos );

			if ( !result.StartedSolid )
			{
				Entity.Position = pos;
				return false;
			}
		}

		StuckTries++;

		return true;
	}

	/// <summary>
	/// Traces the bbox and returns the trace result.
	/// LiftFeet will move the start position up by this amount, while keeping the top of the bbox at the same 
	/// position. This is good when tracing down because you won't be tracing through the ceiling above.
	/// </summary>
	public virtual TraceResult TraceBBox( Vector3 start, Vector3 end, float liftFeet = 0.0f )
	{
		var mins = Entity.CollisionBounds.Mins;
		var maxs = Entity.CollisionBounds.Maxs;

		if ( liftFeet > 0 )
		{
			start += Vector3.Up * liftFeet;
			maxs = maxs.WithZ( maxs.z - liftFeet );
		}

		var tr = Trace.Ray( start, end )
					.Size( mins, maxs )
					.WithAnyTags( "solid", "playerclip", "passbullets", "player" )
					.Ignore( Entity )
					.Run();

		return tr;
	}
}