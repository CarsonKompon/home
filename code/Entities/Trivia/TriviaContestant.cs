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

	public override void Spawn()
	{
		base.Spawn();

		//TEMPORARY, we need a contestant panel model
		SetModel( "models/sbox_props/wooden_crate/wooden_crate.vmdl_c" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}

	public async void FindTriviaGame()
	{
		await GameTask.DelaySeconds( 3 );

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
		if ( Contester.IsValid() ) return;

		joiner.Controller = new TriviaController()
		{
			TriviaPanel = this
		};

		Contester = joiner;
	}

	public void ContesterLeave()
	{
		Game.AssertServer();
		if ( lockPanel ) return;

		if ( Contester.Controller is not HomeWalkController )
			Contester.Controller = new HomeWalkController();

		Log.Info( "Leave success" );

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
		else
			OptionChosen = option;
	}

	public void ResetOptions()
	{
		OptionChosen = -1;
		OptionsChosen.Clear();
	}

	public void ForceLeave()
	{
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
		return true;
	}

	public void DoPostLoading()
	{
		FindTriviaGame();
	}
}
