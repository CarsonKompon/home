using Sandbox;
using Sandbox.UI;

namespace ArcadeZone;

[Library]
public partial class AZHud : HudEntity<RootPanel>
{
    public AZHud()
    {
        if(!Game.IsClient) return;

        RootPanel.StyleSheet.Load("/UI/ArcadeZoneHud.scss");

        RootPanel.AddChild<AZChatBox>();
        RootPanel.AddChild<AZVoiceList>();
        RootPanel.AddChild<AZVoiceSpeaker>();

        RootPanel.AddChild<NotificationPanel>();
    }
}