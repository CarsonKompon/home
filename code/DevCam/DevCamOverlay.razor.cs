using Sandbox;
using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Home.DevCam;

internal class DevCamPP : Sandbox.Effects.ScreenEffects { }

internal class DevCamDof : Sandbox.Effects.DepthOfField { }

/// <summary>
/// A user interface to adjust various post processing effects while in DevCamera mode
/// </summary>
internal partial class DevCamOverlay
{
	public DevCamOverlay()
	{
		Hide();
	}

	void HighlightBottom( string name )
	{
		var menu = Children.FirstOrDefault( x => x.HasClass( "menubar" ) );

		foreach ( var child in menu.Children.Where( x => x.HasClass( "button" ) ) )
		{
			child.SetClass( "active", child.GetAttribute( "for" ) == name );
		}
	}

	public override void Tick()
	{
		base.Tick();

		Style.Cursor = OnMouseMoved == null ? "" : "pointer"; // TODO - dropper
	}

	public void Activated()
	{
		Camera.Main.FindOrCreateHook<Home.DevCam.DevCamDof>();
		Camera.Main.FindOrCreateHook<Home.DevCam.DevCamPP>();
	}

	public void Deactivated()
	{
		Hide();

		Camera.Main.RemoveAllHooks<Home.DevCam.DevCamDof>();
		Camera.Main.RemoveAllHooks<Home.DevCam.DevCamPP>();
	}

	public void Show()
	{
		SetClass( "hidden", false );
	}

	public void Hide()
	{
		SetClass( "hidden", true );
	}

	internal static Action<MousePanelEvent> OnMouseClicked;

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if ( e.Target != this )
			return;

		OnMouseClicked?.Invoke( e );
	}

	internal static Action<MousePanelEvent> OnMouseMoved;

	protected override void OnMouseMove( MousePanelEvent e )
	{
		base.OnMouseMove( e );

		if ( e.Target != this )
			return;

		OnMouseMoved?.Invoke( e );
	}
}
