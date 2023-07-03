namespace Home;

public class PlayermodelButton : Panel
{

	public HomePlayermodel Playermodel { get; set; }
	public bool HasVariations { get; set; }

	public Panel ImagePanel;

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		if ( Playermodel == null ) return;

		DeleteChildren( true );

		ImagePanel = new Panel( this, "image" );
		ImagePanel.Style.BackgroundImage = Playermodel.Texture;

		if( HasVariations )
		{
			AddClass( "has-variations" );
			Add.Icon( "palette", "variations-icon" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		var parent = Ancestors.OfType<Avatar>().FirstOrDefault();
		if ( parent == null ) return;

		SetClass( "active", false );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if(Game.LocalPawn is HomePlayer player)
		{
			ConsoleSystem.Run("home_playermodel", Playermodel.Name);
		}
	}

}
