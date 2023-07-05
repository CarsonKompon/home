namespace Home.Games.Trivia;

[Library( "home_game_trivia_screen" )]
[Title( "Trivia Screen" ), Description( "The Trivia screen for end users" ), Icon( "screenshot_monitor" ), Category( "Trivia" )]
[HammerEntity, BoundsHelper( "screenSizeA", "ScreenSizeB" )]
public class TriviaScreen : Entity
{
	[Property( "screenSizeA" ), DefaultValue("-50 -25")]
	public Vector2 ScreenSizeA { get; set; }
	
	[Property( "screenSizeB" ), DefaultValue( "50 25" )]
	public Vector2 ScreenSizeB { get; set; }

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void ClientSpawn()
	{
		base.ClientSpawn();

		new TriviaWorldPanel();
	}
}
