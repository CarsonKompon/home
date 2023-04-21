using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class AZChatSettings : Panel
	{
        public AZChatSettingsEntry SettingAvatars { get; internal set; }
        public AZChatSettingsEntry SettingFontSize { get; internal set; }

		public AZChatSettings()
		{
            // AVATARS
            Switch toggleAvatars = new Switch();
            toggleAvatars.Value = !Cookie.Get<bool>("arcadezone.chat.hide-avatars", false);
            SettingAvatars = new AZChatSettingsEntry( "Avatars", toggleAvatars );
            SettingAvatars.Control.AddEventListener( "onchange", onSwitchAvatars );
            AddChild(SettingAvatars);
            onSwitchAvatars();

            // FONT SIZE
            Slider sliderFontSize = new Slider();
            sliderFontSize.MinValue = 10;
            sliderFontSize.MaxValue = 24;
            sliderFontSize.Step = 1;
            sliderFontSize.Value = Cookie.Get<float>("arcadezone.chat.font-size", 14);
            SettingFontSize = new AZChatSettingsEntry( "Font Size", sliderFontSize );
            SettingFontSize.Control.AddEventListener( "onchange", onSliderFontSize );
            AddChild(SettingFontSize);
            onSliderFontSize();
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

        private void onSliderFontSize()
        {
            Slider slider = SettingFontSize.Control as Slider;
            AZChatBox.Current.Style.FontSize = slider.Value;
            Cookie.Set( "arcadezone.chat.font-size", slider.Value );
        }

	}
}
