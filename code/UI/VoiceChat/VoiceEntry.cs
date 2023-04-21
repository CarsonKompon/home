using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

namespace ArcadeZone;

public class AZVoiceEntry : Panel
{
	public Friend Friend;

	readonly Panel NameContainer;
	readonly Image Avatar;

	private List<Label> NameLetters = new List<Label>();

	private float VoiceLevel = 0.0f;
	private float TargetVoiceLevel = 0;

	RealTimeSince timeSincePlayed;

	public AZVoiceEntry( Panel parent, long steamId )
	{
		Parent = parent;

		Friend = new Friend( steamId );

		Avatar = Add.Image( "", "avatar" );
		Avatar.SetTexture( $"avatar:{steamId}" );

		NameContainer = Add.Panel( "name" );

		InitLetters();
	}

	private void InitLetters()
	{
		for(int i=0; i<Friend.Name.Length; i++)
		{
			NameLetters.Add(NameContainer.Add.Label( Friend.Name[i].ToString(), "name-letter" ));
		}
	}

	public void Update( float level )
	{
		timeSincePlayed = 0;
		if(NameLetters.Count == 0) InitLetters();
		TargetVoiceLevel = level;
	}

	public override void Tick()
	{
		base.Tick();

		if ( IsDeleting )
			return;

		var SpeakTimeout = 2.0f;
		var timeoutInv = 1 - (timeSincePlayed / SpeakTimeout);
		timeoutInv = MathF.Min( timeoutInv * 2.0f, 1.0f );

		if ( timeoutInv <= 0 )
		{
			Delete();
			return;
		}

		VoiceLevel = VoiceLevel.LerpTo( TargetVoiceLevel, Time.Delta * 40.0f );
		float level = (VoiceLevel * -32.0f);

		if(VoiceLevel > 0.02f) // TALKING
		{
			ColorHsv rainbow = new ColorHsv(((Friend.Id % 360) + (Time.Now*60f)) % 360f, 0.6f, 0.9f, 0.8f);
			Style.BackgroundColor = Color.Lerp(Color.Black, rainbow, 0.8f);
			for(int i=0; i<NameLetters.Count; i++)
			{
				NameLetters[i].Style.Top = MathF.Sin(Time.Now*20f + i) * level/2;
			}
			NameContainer.Style.FontColor = rainbow.WithSaturation(0.2f).WithValue(1f).WithAlpha(1f);
		}
		else // NOT TALKING
		{
			Style.BackgroundColor = Color.Lerp(Color.Black, Color.Transparent, 0.8f);
			for(int i=0; i<NameLetters.Count; i++)
			{
				NameLetters[i].Style.Top = 0f;
			}
			NameContainer.Style.FontColor = Color.White;
		}

		Style.Left = level;
	}
}
