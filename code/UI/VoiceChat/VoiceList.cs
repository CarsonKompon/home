using Sandbox;
using Sandbox.UI;
using System.Linq;

namespace ArcadeZone;

public class AZVoiceList : Panel
{
	public static AZVoiceList Current { get; internal set; }

	public AZVoiceList()
	{
		Current = this;
		StyleSheet.Load( "/UI/VoiceChat/VoiceList.scss" );
	}

    public override void Tick()
	{
        if(Voice.IsRecording)
        {
            var entry = ChildrenOfType<AZVoiceEntry>().FirstOrDefault( x => x.Friend.Id == Game.LocalClient.SteamId );
            if ( entry == null ) entry = new AZVoiceEntry( this, Game.LocalClient.SteamId );

            entry.Update( Voice.Level );
        }
	}

	public void OnVoicePlayed( long steamId, float level )
	{
		var entry = ChildrenOfType<AZVoiceEntry>().FirstOrDefault( x => x.Friend.Id == steamId );
		if ( entry == null ) entry = new AZVoiceEntry( this, steamId );

		entry.Update( level );
	}
}
