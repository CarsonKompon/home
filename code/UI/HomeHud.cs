using Sandbox;
using Sandbox.UI;

namespace Home;

[Library]
public partial class HomeHud : HudEntity<RootPanel>
{
    public HomeHud()
    {
        if(!Game.IsClient) return;

        RootPanel.StyleSheet.Load("/UI/HomeHud.scss");

        RootPanel.AddChild<HomeGUI>();

        RootPanel.AddChild<HomeChatBox>();
        RootPanel.AddChild<HomeVoiceList>();
        RootPanel.AddChild<HomeVoiceSpeaker>();

        RootPanel.AddChild<NotificationPanel>();
    }
}