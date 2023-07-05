using Home.Interfaces;

namespace Home.Games.Trivia;

[Library("home_game_trivia_contestant")]
[Title("Contestant Panel"), Description("The trivia panel for a contestant to play"), Icon( "person" ), Category( "Trivia" )]
[HammerEntity, EditorModel( "models/sbox_props/wooden_crate/wooden_crate.vmdl_c" )]
public partial class TriviaContestant : ModelEntity, IUse, IEntityPostLoad
{
	[Property, Description("The trivia game where contestants are playing on")]
	public EntityTarget TargetGame { get; set; }

	public HomePlayer Contester { get; set; }
	public QuestionStruct ActiveQuestion { get; set; }
	public int OptionChosen { get; set; } = -1;
	public List<int> OptionsChosen { get; set; } = new();

	public TriviaGame MainGame;
	public bool LockAnswer { get; set; }

	//Locks the panel in the event something broke on start
	bool lockPanel = false;

	[Net] public int Score { get; set; }
	[Net] public int CorrectStreak { get; set; }

	[GameEvent.Tick.Server]
	public void DebugInfo()
	{
		if ( Contester == null ) return;

		Vector2 screenPos = new Vector2( 75, 75 );
		DisplayToContestant( To.Single( Contester ), "TRIVIA", screenPos, 0, Color.White );

		var question = MainGame.GetActiveQuestion();

		double timer = Math.Round( MainGame.TriviaTime, 1 );

		switch ( MainGame.GameStatus )
		{
			case TriviaGame.TriviaStatus.Idle:
				DisplayToContestant( To.Single( Contester ), "STATUS: Waiting for players", screenPos, 1, Color.Yellow );
				break;
			case TriviaGame.TriviaStatus.Starting:
				DisplayToContestant( To.Single( Contester ), $"STATUS:Starting in: {timer}", screenPos, 1, Color.Yellow );
				break;
			case TriviaGame.TriviaStatus.Active:
				DisplayToContestant( To.Single( Contester ), $"STATUS: Active", screenPos, 1, Color.Green );
				DisplayToContestant( To.Single( Contester ), $"QUESTION {question.Question}", screenPos, 2, Color.Yellow );
				break;
			case TriviaGame.TriviaStatus.Post:
				DisplayToContestant( To.Single( Contester ), $"STATUS: Game Finished", screenPos, 1, Color.Red );
				break;
		}

		if ( MainGame.GameStatus == TriviaGame.TriviaStatus.Active )
		{
			if(MainGame.RoundStatus == TriviaGame.TriviaRoundStatus.Waiting)
			{
				QuestionStruct.TypeEnum type = question.QuestionType;
				bool selected = type != QuestionStruct.TypeEnum.MultiChoice ?
					OptionChosen != -1 : OptionsChosen.Count > 0;

				DisplayToContestant( To.Single( Contester ), $"Time: {timer}", screenPos, 3, Color.Yellow );
				DisplayToContestant( To.Single( Contester ), $"Answered: {selected}", screenPos, 4, Color.Yellow );
				
				for ( int i = 0; i < question.Answers.Length; i++ )
				{
					Color selectCol = Color.White;

					if (type == QuestionStruct.TypeEnum.MultiChoice )
						selectCol = OptionsChosen.Contains( i ) ? Color.Green : Color.Red;
					else
						selectCol = i == OptionChosen-1 ? Color.Green : Color.Red;

					DisplayToContestant( To.Single( Contester ), $"{i+1}: {question.Answers[i].Answer}", screenPos, 6+i, selectCol );
				}
			}
			else if (MainGame.RoundStatus == TriviaGame.TriviaRoundStatus.PreReveal )
			{
				DisplayToContestant( To.Single( Contester ), $"Revealing...", screenPos, 4, Color.Yellow );
			}
			else
			{
				int answer = MainGame.GetActiveQuestion().Answers.Where( a => a.IsCorrect )
					.FirstOrDefault().GetCorrectOptionInt();

				bool wasCorrect = answer == OptionChosen;

				DisplayToContestant( To.Single( Contester ), $"Was correct: {wasCorrect}", screenPos, 4, Color.Yellow );
			}
		}
	}

	[ClientRpc]
	public void DisplayToContestant(string msg, Vector2 pos, int line, Color color)
	{
		DebugOverlay.ScreenText( msg, pos, line, color );
	}

	public override void Spawn()
	{
		base.Spawn();

		//TEMPORARY, we need a contestant panel model
		SetModel( "models/sbox_props/wooden_crate/wooden_crate.vmdl_c" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	public void FindTriviaGame()
	{
		MainGame = TargetGame.GetTarget( null ) as TriviaGame;

		if ( MainGame == null || !MainGame.IsValid )
		{
			lockPanel = true;
			Log.Error( $"HOME: {Name} has an invalid MainGame target" );
		}
		else
		{
			LockAnswer = false;
			MainGame.AddContestPanel( this );
		}
	}

	public void ContesterJoin(HomePlayer joiner)
	{
		Game.AssertServer();

		if ( lockPanel ) return;
		if ( MainGame.IsPlaying ) return;

		joiner.Controller = new TriviaController()
		{
			TriviaPanel = this
		};

		Contester = joiner;

		if ( MainGame.CanStartGame() )
			MainGame.BeginGame();
	}

	public void ContesterLeave()
	{
		Game.AssertServer();
		if ( lockPanel ) return;

		if ( Contester.Controller is not HomeWalkController )
			Contester.Controller = new HomeWalkController();

		Score = 0;
		CorrectStreak = 0;
		ResetOptions();

		LockAnswer = false;
		Contester = null;
	}

	public void SelectOption(int option)
	{
		if ( LockAnswer ) return;

		if( ActiveQuestion.QuestionType == QuestionStruct.TypeEnum.MultiChoice )
		{
			if ( OptionsChosen.Contains( option ) )
				OptionsChosen.Remove( option );
			else
				OptionsChosen.Add( option );
		}
		else if (ActiveQuestion.QuestionType == QuestionStruct.TypeEnum.TrueOrFalse )
		{
			if ( option == 3 )
				option = 1;

			if ( option == 4 )
				option = 2;
		}
		else
			OptionChosen = option;
	}

	/// <summary>
	/// Gets the option chosen
	/// </summary>
	/// <returns></returns>
	public int GetOptionChosen() => OptionChosen;

	/// <summary>
	/// Gets all options chosen, used for multi-choice questions
	/// </summary>
	public List<int> GetOptionsChosen() => OptionsChosen;

	public void AddScore( int amount ) => Score += amount;

	public void ResetOptions()
	{
		LockAnswer = false;
		OptionChosen = -1;
		OptionsChosen.Clear();
	}

	public void ForceLeave()
	{
		Score = 0;
		CorrectStreak = 0;
		ResetOptions();

		LockAnswer = false;
		Contester = null;
	}

	public bool OnUse( Entity user )
	{
		ContesterJoin( user as HomePlayer );

		return false;
	}

	public bool IsUsable( Entity user )
	{
		return Contester == null;
	}

	public void DoPostLoading()
	{
		FindTriviaGame();
	}
}
