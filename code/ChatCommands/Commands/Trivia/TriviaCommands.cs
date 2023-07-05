namespace Home.Commands;

//Starting
#region
[ChatCommand]
public class TriviaStart : ChatCommandAttribute
{
	public TriviaStart()
	{
		Name = "quizstart";
		Description = "Starts a trivia game";
		Arguments = new List<ChatArgument>();
	}

	public override void Run( IClient client )
	{
		//if ( Game.IsServerHost && !Game.IsDedicatedServer ) return;

		var player = client.Pawn as HomePlayer;

		if( player.Controller is TriviaController tc )
		{
			TriviaGame game = tc.TriviaPanel.MainGame;
			game.StartUpGame();
		}
		else
		{
			TriviaGame game = FindNearestTrivia( player.Position );
			if ( game.GetActiveContestants().Count > 0 ) return;

			game.StartUpGame();
		}

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
#endregion
