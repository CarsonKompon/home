namespace Home;

public class TriviaController : HomePawnController
{
    public TriviaContestant TriviaPanel { get; set; }
	[ClientInput] public AnswerStruct.OptionEnum SelectedOption { get; set; }

	public override void Simulate()
    {
        base.Simulate();

        if( TriviaPanel == null ) return;

        WishVelocity = Vector3.Zero;
        Velocity = Vector3.Zero;

		var pos = TriviaPanel.Children[0].Position;
        
		if( !pos.IsNaN )
        {
			Position = pos;
        }
        else
        {
            Position = TriviaPanel.Position + Vector3.Down * 10 + TriviaPanel.Rotation.Forward * 40f + Vector3.Up * 10f;
        }

		if ( Input.Pressed( "crouch" ) || Input.Pressed( "Jump" ) )
        {
			TriviaPanel.ContesterLeave();
        }

        BuildInput();

		if( SelectedOption != AnswerStruct.OptionEnum.UnSelected)
		{
			TriviaPanel.SelectOption( (int)SelectedOption );
			SelectedOption = AnswerStruct.OptionEnum.UnSelected;
		}
    }

	//TEMPORARY, until we get proper inputs for the contestant
	public AnswerStruct.OptionEnum InputToOptionEnum()
	{
		if ( Input.Pressed( "Forward" ) )
			return AnswerStruct.OptionEnum.A;

		if ( Input.Pressed( "Backward" ) )
			return AnswerStruct.OptionEnum.C;
		
		if ( Input.Pressed( "Left" ) )
			return AnswerStruct.OptionEnum.B;

		if ( Input.Pressed( "Right" ) )
			return AnswerStruct.OptionEnum.D;

		return AnswerStruct.OptionEnum.UnSelected;
	}

	public override void BuildInput()
	{
		base.BuildInput();

		if(Pawn is HomePlayer)
		{
			var option = InputToOptionEnum();

			if ( option != AnswerStruct.OptionEnum.UnSelected )
				SelectedOption = option;
		}
	}
}
