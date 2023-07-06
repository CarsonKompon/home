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

	[Property, MinMax(15, 60)]
	public int BaseRoundTime { get; set; } = 30;
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
			ResetGame();

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
	/// Starts up the game, this doesn't begin gameplay
	/// </summary>
	public void StartUpGame()
	{
		//TriviaTime = 20.0f;
		TriviaTime = 5.0f;
		GameStatus = TriviaStatus.Starting;
	}

	/// <summary>
	/// Begins gameplay, not the same as starting
	/// </summary>
	public void BeginGame()
	{
		SetUpGame();
		TriviaTime = BaseRoundTime;
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
		List<TriviaContestant> contestants = GetActiveContestants();

		QuestionStruct question = GetActiveQuestion();

		var answer = question.Answers.Where( a => a.IsCorrect ).FirstOrDefault();

		foreach ( var contester in contestants )
		{
			float timeAnswered = (contester.Contester.Controller as TriviaController).TookToAnswer;

			if ( question.QuestionType == QuestionStruct.TypeEnum.MultiChoice )
			{
				var chosen = contester.GetOptionsChosen();
				var answers = question.Answers.ToList();

				int correct = 0;
				int incorrect = 0;

				for ( int i = 1; i <= answers.Count; i++ )
				{
					if ( chosen.Contains( i ) )
					{
						if ( answers[i - 1].IsCorrect )
							correct++;
						else
							incorrect++;
					}
					else if ( !chosen.Contains( i ) && answers[i-1].IsCorrect )
						incorrect++;
				}

				if ( correct > 0 )
				{
					if ( incorrect == 0 )
					{
						contester.CorrectStreak++;
						contester.AddScore( CalculateGivingPoints( contester.CorrectStreak, timeAnswered ) );
					}
					else
						contester.AddScore( CalculateGivingPoints( (correct - incorrect) + 1, timeAnswered, true ) );
				}
			}
			else
			{
				if ( contester.GetOptionChosen() == (int)answer.Option)
				{
					contester.CorrectStreak++;
					contester.AddScore( CalculateGivingPoints( contester.CorrectStreak, timeAnswered ) );
				}
				else
					contester.CorrectStreak = 0;
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
	/// <param name="amount">The amount to multiply or divide</param>
	/// <param name="timeLocked">The time since the answer was locked</param>
	/// <param name="mixed">If the player has both a incorrect and a corret answer</param>
	/// <returns>The score calculated</returns>
	int CalculateGivingPoints(int amount, float timeLocked, bool mixed = false)
	{
		int points = 5;

		if ( amount > 2 && !mixed )
			points *= amount - 1;

		if( !mixed )
			points += (int)Math.Ceiling( BaseRoundTime / timeLocked );

		if ( mixed )
			points = (int)MathF.Round( points / amount );

		var player = GetActiveContestants()[0].Contester;

		GetActiveContestants()[0].DisplayToContestant( To.Single( player ), $"Points earned: {points}", new Vector2( 125, 75 ), 0, Color.Yellow, 7.5f );

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
		RoundStatus = TriviaRoundStatus.Waiting;
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
