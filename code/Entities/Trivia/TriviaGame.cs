namespace Home.Games.Trivia;

[Library("home_game_trivia")]
[Title("Trivia Game"), Description("The Trivia game entity"), Icon( "videogame_asset" ), Category("Trivia")]
[HammerEntity]
public partial class TriviaGame : Entity, IEntityPostLoad
{
	[Property]
	public EntityTarget TriviaScreenTarget { get; set; }

	[Property, Range(5, 25, 5)]
	public int MaxRounds { get; set; } = 5;

	public int CurRound { get; set; }

	public TriviaScreen TriviaScreen;

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

	[Net, Change(nameof( OnGameChange ))] public TriviaStatus GameStatus { get; set; } = TriviaStatus.Idle;

	[Net, Change(nameof( OnRoundChange ) )] public TriviaRoundStatus RoundStatus { get; set; } = TriviaRoundStatus.Waiting;

	public bool IsPlaying => GameStatus == TriviaStatus.Active;

	public List<QuestionStruct> Questions { get; set; }
	public List<TriviaContestant> ContestantPanels { get; set; } = new();
	[Net] public TimeUntil TriviaTime { get; set; }

	TriviaWorldPanel worldPanel;

	public void OnGameChange( TriviaStatus oldStatus, TriviaStatus newStatus )
	{
		if ( Game.IsServer )
		{
			UpdateGameClient( To.Everyone, newStatus );
		}
	}

	public void OnRoundChange( TriviaRoundStatus oldStatus, TriviaRoundStatus newStatus )
	{
		if ( Game.IsServer )
		{
			UpdateRoundClient( To.Everyone, newStatus );
		}
	}

	[ClientRpc]
	void UpdateGameClient( TriviaStatus updated )
	{
		worldPanel.UpdateGameStatus( updated );
	}

	[ClientRpc]
	void UpdateRoundClient( TriviaRoundStatus updated )
	{
		worldPanel.UpdateRoundStatus( updated );
	}

	public void DoPostLoad()
	{
		TriviaScreen = TriviaScreenTarget.GetTarget( null ) as TriviaScreen;

		if( TriviaScreen == null || !TriviaScreen.IsValid())
		{
			Log.Error( $"HOME: {Name} is missing the trivia screen" );
		}
	}

	public override void Spawn()
	{
		base.Spawn();
		Questions = new();
		CurRound = 0;
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		worldPanel = new TriviaWorldPanel();
	}

	public void AddContestPanel( TriviaContestant newPanel )
	{
		ContestantPanels.Add( newPanel );
	}

	[GameEvent.Tick.Server]
	public void TickTriviaGame()
	{
		if ( GameStatus == TriviaStatus.Idle ) return;

		if ( ShouldCancelGame() )
			CancelGame();

		if ( TriviaTime > 0.0f ) return;

		switch( GameStatus )
		{
			case TriviaStatus.Starting: BeginGame(); return;
			case TriviaStatus.Active: UpdateState(); return;
			case TriviaStatus.Post: ResetGame(); return;
		}
	}

	//Getters
	#region
	/// <summary>
	/// Gets active contestants
	/// </summary>
	/// <returns>List of contestants playing</returns>
	public List<TriviaContestant> GetActiveContestants() => ContestantPanels.Where( p => p.Contester.IsValid() ).ToList();

	/// <summary>
	/// Gets the active question
	/// </summary>
	public QuestionStruct GetActiveQuestion()
	{
		if ( !IsPlaying ) return QnASheet.DummyQuestion[0];

		return Questions[CurRound - 1];
	}

	#endregion

	//Start/End gameplay
	#region
	/// <summary>
	/// Can the game start up
	/// </summary>
	public bool CanStartGame()
	{
		return GetActiveContestants().Count >= 2;
	}

	/// <summary>
	/// If the game should be cancelled, either due to players leaving etc.
	/// </summary>
	public bool ShouldCancelGame()
	{
		if ( GetActiveContestants().Count == 0 ) return true;

		return false;
	}

	/// <summary>
	/// Cancels the game therefore restoring to idle state
	/// </summary>
	public void CancelGame()
	{
		GameStatus = TriviaStatus.Idle;
		RoundStatus = TriviaRoundStatus.Waiting;
	}

	/// <summary>
	/// Starts up the game, this doesn't begin gameplay
	/// </summary>
	public void StartUpGame()
	{
		TriviaTime = 20.0f;
		GameStatus = TriviaStatus.Starting;
	}

	/// <summary>
	/// Begins gameplay, not the same as starting
	/// </summary>
	public void BeginGame()
	{
		SetUpGame();
		TriviaTime = 45.0f;
		ContestantPanels.ForEach( l => l.ResetOptions() );
		GameStatus = TriviaStatus.Active;
	}

	/// <summary>
	/// Ends the game
	/// </summary>
	public void EndGame()
	{
		GameStatus = TriviaStatus.Post;
		RoundStatus = TriviaRoundStatus.Waiting;
	}
	#endregion

	//Gameplay flow
	#region
	/// <summary>
	/// Updates the round status
	/// </summary>
	public void UpdateState()
	{
		switch( RoundStatus )
		{
			case TriviaRoundStatus.Waiting: DoPreReveal(); return;
			case TriviaRoundStatus.PreReveal: DoReveal(); return;
			case TriviaRoundStatus.Reveal: NextRound(); return;
		}
	}

	/// <summary>
	/// Sets up the trivia game, called when the game begins
	/// </summary>
	void SetUpGame()
	{
		QnASheet.ResetQuestions();

		CurRound = 1;

		for ( int i = 0; i < MaxRounds; i++ )
			Questions.Add( GenerateQuestion() );
	}

	/// <summary>
	/// Pre reveal, this is before the answer is shown
	/// </summary>
	public void DoPreReveal()
	{
		ContestantPanels.ForEach( l => l.LockAnswer = true );
		TriviaTime = 5.0f;

		RoundStatus = TriviaRoundStatus.PreReveal;
	}

	/// <summary>
	/// Reveal, this is where the answer is shown
	/// </summary>
	public void DoReveal()
	{
		DoScoreUpdate();
		RoundStatus = TriviaRoundStatus.Reveal;
		TriviaTime = 7.5f;
	}

	/// <summary>
	/// Move onto the next round or ends the game
	/// </summary>
	public void NextRound()
	{
		CurRound++;
		ContestantPanels.ForEach( l => l.ResetOptions() );

		TriviaTime = 45.0f;
		RoundStatus = TriviaRoundStatus.Waiting;
	}

	/// <summary>
	/// Updates each contestants score
	/// </summary>
	public void DoScoreUpdate()
	{
		List<TriviaContestant> players = GetActiveContestants();

		QuestionStruct question = GetActiveQuestion();

		AnswerStruct answer = question.Answers.Where( a => a.IsCorrect ).FirstOrDefault();

		foreach ( var player in players )
		{
			if ( question.QuestionType == QuestionStruct.TypeEnum.MultiChoice )
			{

			}
			else
			{
				if (player.GetOptionChosen() == (int)answer.Option )
				{
					player.CorrectStreak++;
					player.AddScore( CalculateGivingPoints(player.CorrectStreak) );
				}
				else
				{
					player.CorrectStreak = 0;
				}
			}
		}
	}

	[ClientRpc]
	public void DisplayToUser()
	{

	}

	[ClientRpc]
	public void DisplayAnswer()
	{

	}

	[ClientRpc]
	public void DisplayEndResults()
	{

	}

	/// <summary>
	/// Calculates score before giving to the player
	/// </summary>
	/// <param name="streak">The players streak from continous correct answers</param>
	/// <returns>The score calculated</returns>
	int CalculateGivingPoints(int streak = 0)
	{
		int points = 5;

		if ( streak > 2 )
			points *= streak - 1;

		//TODO: Multi-choice conditions for points

		return points;
	}

	/// <summary>
	/// Generates a question for the game
	/// </summary>
	public QuestionStruct GenerateQuestion()
	{
		return QnASheet.TakeQuestion();
	}
	#endregion

	//Resetting
	#region
	/// <summary>
	/// Resets the game for the next
	/// </summary>
	public void ResetGame()
	{
		EjectPlayers();
		GameStatus = TriviaStatus.Idle;
	}

	/// <summary>
	/// Forces contestants out of the panel
	/// </summary>
	public void EjectPlayers()
	{
		foreach ( var contest in ContestantPanels.Where(x => x.Contester is HomePlayer).ToArray() )
			contest.ForceLeave();
	}
	#endregion
}
