namespace Sandbox.UI;

public class AZVoiceSpeaker : Label
{
	private float VoiceLevel = 0.0f;

	public AZVoiceSpeaker()
	{
		StyleSheet.Load( "/UI/VoiceChat/VoiceSpeaker.scss" );
		Text = "mic";
	}

	public override void Tick()
	{
		VoiceLevel = VoiceLevel.LerpTo( Voice.Level, Time.Delta * 40.0f );
		SetClass( "active", Voice.IsRecording );

		var tr = new PanelTransform();
		tr.AddScale( 1.0f.LerpTo( 1.2f, VoiceLevel ) );
		Style.Transform = tr;
	}
}
