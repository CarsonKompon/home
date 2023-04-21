using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcadeZone;


public partial class ArcadeZoneGame : GameManager
{
	public ArcadeZoneGame()
	{
		if (Game.IsServer)
		{
			// Create the HUD
			_ = new ArcadeZoneHud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		var player = new ArcadeZonePlayer(client);
		player.Respawn();

		client.Pawn = player;
	}

}
