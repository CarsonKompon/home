namespace Home.Games.Trivia;

[Library("home_game_trivia_contestant")]
[Title("Contestant Panel"), Description("The trivia panel for a contestant to play"), Icon( "person" )]
[HammerEntity, EditorModel( "models/sbox_props/wooden_crate/wooden_crate.vmdl_c" )]
public class TriviaContestant : ModelEntity
{
	[Property, Description("The trivia game where contestants are playing on"), FGDType( "target_destination" )]
	public TriviaGame MainGame { get; set; }

	public HomePlayer Contester { get; set; }
	public QuestionStruct ActiveQuestion { get; set; }
	public int OptionChosen { get; set; } = -1;
	public List<int> OptionsChosen { get; set; } = new();

	public bool LockAnswer { get; set; }

	//Locks the panel in the event something broke on start
	bool lockPanel => MainGame is not TriviaGame;

	public override void Spawn()
	{
		base.Spawn();

		if( lockPanel )
			Log.Error( $"HOME: {Name} has an invalid MainGame target" );
		else
		{
			LockAnswer = false;
			MainGame.AddContestPanel( this );
		}
	}

	public void ContesterJoin(HomePlayer joiner)
	{
		if ( lockPanel ) return;
		if ( MainGame.IsPlaying ) return;

		Contester = joiner;
	}

	public void ContesterLeft()
	{
		if ( lockPanel ) return;

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

	[Event.Hotload]
	void EntityHotload()
	{
		if ( lockPanel ) return;

		MainGame.AddContestPanel( this );
	}
}
