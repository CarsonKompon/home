using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace Home
{
	public partial class HomeGUI : Panel
	{
        public static HomeGUI Current;

        public AvatarHud AvatarHud;
		public Label MoneyLabel;
        public Label LocationLabel;

		public HomeGUI()
        {
            Current = this;

            StyleSheet.Load( "/ui/HomeGUI.scss" );

            Add.Panel( "bottom-line" );

            AvatarHud = AddChild<AvatarHud>();
            
            Panel canvas = Add.Panel( "canvas" );

            MoneyLabel = canvas.Add.Label( "$0", "money" );
            LocationLabel = canvas.Add.Label( "Main Plaza", "location");

            Add.Image( "ui/GUI/ui_roof.png", "roof" );

        }

		public override void Tick()
		{
			base.Tick();
                
            if (HomePlayer.Local != null)
            {
                MoneyLabel.Text = $"${HomePlayer.Local.Money}";
                LocationLabel.Text = HomePlayer.Local.Location;
            }
		}

	}
}
