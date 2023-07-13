using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace Home
{
	public partial class NotificationPanel : Panel
	{
        public static NotificationPanel Current;

		public NotificationPanel()
        {
            Current = this;

            StyleSheet.Load( "/ui/notifications/Notifications.scss" );
        }

        [ConCmd.Admin("home_notify", Help = "Notifies all players")]
		public static void Announce( string message, string color = "white", float length = 15f)
        {
            AddEntry( To.Everyone, message, color, length);
        }


		[ClientRpc]
		public static void AddEntry( string message, string color = "", float length = 15f)
		{
			var e = Current.AddChild<NotificationPanelEntry>();
			Current.SetChildIndex(e, 0);

			e.Text.Text = message;
            e.TimeLength = length;

			if(color == "rainbow")
			{
				e.Text.AddClass("rainbow");
			}
			else
			{
				if(color == "") e.Text.Style.FontColor = Color.White;
				else e.Text.Style.FontColor = color;
			}
            
            Audio.Play( "ui.notification" );
		}

	}
}
