namespace Home.UI;

public partial class TriviaWorldPanel
{
	public static TriviaWorldPanel TriviaPanel { get; protected set; }

	public TriviaGame.TriviaStatus ClientGameStatus;
	public TriviaGame.TriviaRoundStatus ClientRoundStatus;

	public TriviaWorldPanel()
	{
		TriviaPanel?.Delete();
		TriviaPanel = null;

		TriviaPanel = this;

		ClientGameStatus = TriviaGame.TriviaStatus.Idle;
		ClientRoundStatus = TriviaGame.TriviaRoundStatus.Waiting;
	}

	public void UpdateGameStatus( TriviaGame.TriviaStatus newStatus ) => ClientGameStatus = newStatus;

	public void UpdateRoundStatus( TriviaGame.TriviaRoundStatus newStatus ) => ClientRoundStatus = newStatus;

	public override void Tick()
	{
		base.Tick();

		DebugOverlay.Text( "Trivia Screen", TriviaPanel.Position );
	}
}
