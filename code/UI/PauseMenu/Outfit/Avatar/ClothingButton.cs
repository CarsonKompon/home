namespace Home;

public class ClothingButton : Panel
{

	public Clothing Clothing { get; set; }
	public bool HasVariations { get; set; }

	public Panel ImagePanel;

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		if ( Clothing == null ) return;

		DeleteChildren( true );

		ImagePanel = new Panel( this, "image" );
		
		if(Clothing.Icon.Path != null && FileSystem.Mounted.FileExists(Clothing.Icon.Path))
		{
			ImagePanel.Style.SetBackgroundImage( Clothing.Icon.Path );
		}
		else
		{
			ImagePanel.Style.SetBackgroundImage( Clothing.ResourcePath.Replace(".clothing", ".png").Replace(".hcloth", ".png") );
		}

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

		var equipped = parent.Container.Has( Clothing );
		var variationEquipped = HasVariations && parent.Container.Clothing.Any( x => x.Parent == Clothing );

		SetClass( "active", equipped || variationEquipped );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		var parent = Ancestors.OfType<Avatar>().FirstOrDefault();
		if ( parent == null ) return;

		parent.Select( Clothing );
	}

}
