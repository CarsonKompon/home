using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



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
            Switch toggleAvatars = new Switch();
            toggleAvatars.Value = !Cookie.Get<bool>("home.chat.hide-avatars", false);
            SettingAvatars = new HomeChatSettingsEntry( "Avatars", toggleAvatars );
            SettingAvatars.Control.AddEventListener( "onchange", onSwitchAvatars );
            AddChild(SettingAvatars);
            onSwitchAvatars();

            // FONT SIZE
            Slider sliderFontSize = new Slider();
            sliderFontSize.MinValue = 10;
            sliderFontSize.MaxValue = 24;
            sliderFontSize.Step = 1;
            sliderFontSize.Value = Cookie.Get<float>("home.chat.font-size", 14);
            SettingFontSize = new HomeChatSettingsEntry( "Font Size", sliderFontSize );
            SettingFontSize.Control.AddEventListener( "onchange", onSliderFontSize );
            AddChild(SettingFontSize);
            onSliderFontSize();

            // CHAT SOUNDS
            Switch toggleChatSounds = new Switch();
            toggleChatSounds.Value = !Cookie.Get<bool>("home.chat.mute", false);
            SettingChatSounds = new HomeChatSettingsEntry( "Chat Blip SFX", toggleChatSounds );
            SettingChatSounds.Control.AddEventListener( "onchange", onSwitchChatSounds );
            AddChild(SettingChatSounds);
            onSwitchChatSounds();
		}

        private void onSwitchAvatars()
        {
            Switch toggle = SettingAvatars.Control as Switch;
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
            Slider slider = SettingFontSize.Control as Slider;
            HomeChatBox.Current.Style.FontSize = slider.Value;
            Cookie.Set( "home.chat.font-size", slider.Value );
        }

        private void onSwitchChatSounds()
        {
            Switch toggle = SettingChatSounds.Control as Switch;
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
