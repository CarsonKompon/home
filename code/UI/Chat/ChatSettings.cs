namespace Home
{

	public partial class HomeChatSettings : Panel
	{
        public HomeChatSettingsEntry SettingAvatars { get; internal set; }
        public HomeChatSettingsEntry SettingFontSize { get; internal set; }
        public HomeChatSettingsEntry SettingChatSounds { get; internal set; }

		public HomeChatSettings()
		{
            // AVATARS
            SwitchControl toggleAvatars = new SwitchControl();
            toggleAvatars.Value = !Cookie.Get<bool>("home.chat.hide-avatars", false);
            SettingAvatars = new HomeChatSettingsEntry( "#chat.settings.avatars", toggleAvatars );
            SettingAvatars.Control.AddEventListener( "onchange", onSwitchAvatars );
            AddChild(SettingAvatars);
            onSwitchAvatars();

            // FONT SIZE
            SliderControl sliderFontSize = new SliderControl();
            sliderFontSize.Min = 10;
            sliderFontSize.Max = 24;
            sliderFontSize.Step = 1;
            sliderFontSize.Value = Cookie.Get<float>("home.chat.font-size", 14);
            SettingFontSize = new HomeChatSettingsEntry( "#chat.settings.fontsize", sliderFontSize );
            SettingFontSize.Control.AddEventListener( "onchange", onSliderFontSize );
            AddChild(SettingFontSize);
            onSliderFontSize();

            // CHAT SOUNDS
            SwitchControl toggleChatSounds = new SwitchControl();
            toggleChatSounds.Value = !Cookie.Get<bool>("home.chat.mute", false);
            SettingChatSounds = new HomeChatSettingsEntry( "#chat.settings.chatsfx", toggleChatSounds );
            SettingChatSounds.Control.AddEventListener( "onchange", onSwitchChatSounds );
            AddChild(SettingChatSounds);
            onSwitchChatSounds();
		}

        private void onSwitchAvatars()
        {
            SwitchControl toggle = SettingAvatars.Control as SwitchControl;
            if ( !toggle.Value )
            {
                HomeChatBox.Current.AddClass( "hide-avatars" );
            }
            else
            {
                HomeChatBox.Current.RemoveClass( "hide-avatars" );
            }
            Cookie.Set( "home.chat.hide-avatars", !toggle.Value );
        }

        private void onSliderFontSize()
        {
            SliderControl slider = SettingFontSize.Control as SliderControl;
            HomeChatBox.Current.Style.FontSize = slider.Value;
            Cookie.Set( "home.chat.font-size", slider.Value );
        }

        private void onSwitchChatSounds()
        {
            SwitchControl toggle = SettingChatSounds.Control as SwitchControl;
            if ( !toggle.Value )
            {
                HomeChatBox.Current.AddClass( "mute" );
            }
            else
            {
                HomeChatBox.Current.RemoveClass( "mute" );
            }
            Cookie.Set( "home.chat.mute", !toggle.Value );
            HomeChatBox.Current.MessageSounds = toggle.Value;
        }

	}
}
