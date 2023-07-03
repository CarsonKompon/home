namespace Home.Games.Trivia;

[Library("home_game_trivia")]
[Title("Trivia Game"), Description("The Trivia game entity"), Icon( "videogame_asset" )]
[HammerEntity]
public partial class TriviaGame : Entity
{
	[Property, Range(5, 25, 5)]
	public int MaxRounds { get; set; } = 5;

	public int CurRound { get; set; }

	public enum TriviaStatus
	{
		Idle,
		Starting,
		Active,
		Post,
	}

	public enum TriviaRoundStatus
	{
		Waiting,
		PreReveal,
		Reveal
	}

	[Net] public TriviaStatus GameStatus { get; set; } = TriviaStatus.Idle;

	[Net] public TriviaRoundStatus RoundStatus { get; set; } = TriviaRoundStatus.Waiting;

	public bool IsPlaying => GameStatus == TriviaStatus.Active;

	public List<QuestionStruct> Questions { get; set; }
	public List<TriviaContestant> ContestantPanels { get; set; } = new();
	[Net] public TimeUntil TriviaTime { get; set; }

	public override void Spawn()
	{
		base.Spawn();
		Questions = new();
		CurRound = 0;
	}

	public void AddContestPanel( TriviaContestant newPanel )
	{
		ContestantPanels.Add( newPanel );
	}

	[GameEvent.Tick.Server]
	public void TickTriviaGame()
	{
		if ( GameStatus == TriviaStatus.Idle ) return;

		switch( GameStatus )
		{
			case TriviaStatus.Starting: BeginGame(); return;
			case TriviaStatus.Active: UpdateState();  return;
			case TriviaStatus.Post: ResetGame(); return;
		}
	}

	public int CountPlayers()
	{
		int count = 0;

		foreach ( var panel in ContestantPanels )
			if ( panel.Contester is HomePlayer ) count++;
		
		return count;
	}

	//Start/End gameplay
	#region
	public bool CanBeginGame()
	{
		return false;
	}

	public bool ShouldCancelGame()
	{
		if ( CountPlayers() == 0 ) return true;

		return false;
	}

	public void CancelGame()
	{
		GameStatus = TriviaStatus.Idle;
		RoundStatus = TriviaRoundStatus.Waiting;
	}

	public void StartUpGame()
	{
		GameStatus = TriviaStatus.Starting;
	}

	public void BeginGame()
	{
		GameStatus = TriviaStatus.Active;
	}

	public void EndGame()
	{
		GameStatus = TriviaStatus.Post;
		RoundStatus = TriviaRoundStatus.Waiting;
	}
	#endregion

	//Gameplay flow
	#region
	public void UpdateState()
	{
		switch( RoundStatus )
		{
			case TriviaRoundStatus.Waiting: RoundStatus = TriviaRoundStatus.PreReveal; return;
			case TriviaRoundStatus.PreReveal: RoundStatus = TriviaRoundStatus.Reveal; return;
			case TriviaRoundStatus.Reveal: RoundStatus = TriviaRoundStatus.Waiting; return;
		}
	}

	public void SetUpGame()
	{
		QnASheet.ResetQuestions();

		CurRound = 1;

		for ( int i = 0; i < MaxRounds; i++ )
			Questions.Add( GenerateQuestion() );
	}

	public void DoPreReveal()
	{
		ContestantPanels.ForEach( l => l.LockAnswer = !l.LockAnswer );
	}

	public void NextRound()
	{
		CurRound++;
	}

	public QuestionStruct GenerateQuestion()
	{
		return QnASheet.TakeQuestion();
	}
	#endregion

	//Resetting
	#region
	public void ResetGame()
	{
		EjectPlayers();
		GameStatus = TriviaStatus.Idle;
	}

	public void EjectPlayers()
	{
		foreach ( var contest in ContestantPanels.Where(x => x.Contester is HomePlayer).ToArray() )
			contest.ForceLeave();
	}
	#endregion
}
