using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeZone;


public partial class AZGame : GameManager
{
	public AZGame()
	{
		if (Game.IsServer)
		{
			// Create the HUD
			_ = new AZHud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		var player = new AZPlayer(client);
		player.Respawn();

		client.Pawn = player;
	}

}
