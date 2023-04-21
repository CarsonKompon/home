using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class AZChatSettings : Panel
	{
        public AZChatSettingsEntry SettingAvatars { get; internal set; }

		public AZChatSettings()
		{
            Switch toggleAvatars = new Switch();
            toggleAvatars.Value = !Cookie.Get<bool>("arcadezone.chat.hide-avatars", false);
            SettingAvatars = new AZChatSettingsEntry( "Avatars", toggleAvatars );
            SettingAvatars.Control.AddEventListener( "onchange", onSwitchAvatars );
            AddChild(SettingAvatars);
            onSwitchAvatars();
		}

        private void onSwitchAvatars()
        {
            Switch toggle = SettingAvatars.Control as Switch;
            if ( !toggle.Value )
            {
                AZChatBox.Current.AddClass( "hide-avatars" );
            }
            else
            {
                AZChatBox.Current.RemoveClass( "hide-avatars" );
            }
            Cookie.Set( "arcadezone.chat.hide-avatars", !toggle.Value );
        }

	}
}
