namespace Home.UI;

public partial class TriviaWorldPanel
{
	public static TriviaWorldPanel TriviaPanel { get; protected set; }

	public TriviaGame.TriviaStatus ClientGameStatus;
	public TriviaGame.TriviaRoundStatus ClientRoundStatus;

	public TriviaWorldPanel()
	{
		TriviaPanel = this;

		ClientGameStatus = TriviaGame.TriviaStatus.Idle;
		ClientRoundStatus = TriviaGame.TriviaRoundStatus.Waiting;
	}

	public void UpdateGameState( TriviaGame.TriviaStatus newStatus )
	{

	}

	public void UpdateRoundStatus( TriviaGame.TriviaRoundStatus newStatus )
	{

	}
}
