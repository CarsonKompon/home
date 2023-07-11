
using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Home.Util;
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
		
		SetIcon();

		if( HasVariations )
		{
			AddClass( "has-variations" );
			Add.Icon( "palette", "variations-icon" );
		}
	}

	private async void SetIcon()
	{
		ImagePanel.Style.BackgroundImage = await HomeClothing.GetIcon( Clothing );
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
