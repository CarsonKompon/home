namespace Home.Commands;

[ChatCommand]
public class TriviaStart : ChatCommandAttribute
{
	public TriviaStart()
	{
		Name = "Start Trivia Game";
		Description = "Starts a trivia game";
		Arguments = new List<ChatArgument>();
	}

	public override void Run( IClient client )
	{
		if ( !client.IsListenServerHost && !Game.IsDedicatedServer ) return;

		var player = client.Pawn as HomePlayer;

		TriviaGame game = FindNearestTrivia( player.Position );
	}

	TriviaGame FindNearestTrivia(Vector3 playerPos)
	{
		float minDist = 92.0f;

		TriviaGame found = null;

		foreach ( var trivia in Entity.All.OfType<TriviaGame>() )
		{
			if( trivia.Position.Distance( playerPos ) < minDist )
			{
				if ( found == null )
					found = trivia;
				else
				{
					if ( trivia.Position.Distance( playerPos ) > found.Position.Distance( playerPos ) )
						found = trivia;
				}
			}
		}

		return found;
	}
}
