using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ArcadeZone;


public partial class AZGame : GameManager
{
	public static new AZGame Current;

	public List<ChatCommandAttribute> ChatCommands { get; set; }

	public AZGame()
	{
		Current = this;
		
		if (Game.IsServer)
		{
			// Create the HUD
			_ = new AZHud();
		}

		// Load the game's different libraries
		LoadLibraries();
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		var player = new AZPlayer(client);
		player.Respawn();

		client.Pawn = player;
	}

	[Event.Hotload]
	public static void LoadLibraries()
	{
		// Load the chat commands
		Current.ChatCommands = new List<ChatCommandAttribute>();
		foreach(TypeDescription typeDesc in TypeLibrary.GetTypes<ChatCommandAttribute>())
		{
			ChatCommandAttribute command = TypeLibrary.Create<ChatCommandAttribute>(typeDesc.TargetType);
			Current.ChatCommands.Add(command);
		}
	}

}
