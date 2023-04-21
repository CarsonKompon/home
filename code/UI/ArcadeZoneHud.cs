using Sandbox;
using Sandbox.UI;

namespace ArcadeZone;

[Library]
public partial class ArcadeZoneHud : HudEntity<RootPanel>
{
    public ArcadeZoneHud()
    {
        if(!Game.IsClient) return;

        RootPanel.StyleSheet.Load("/UI/ArcadeZoneHud.scss");

        RootPanel.AddChild<ChatBox>();
        RootPanel.AddChild<AZVoiceList>();
        RootPanel.AddChild<AZVoiceSpeaker>();
    }
}