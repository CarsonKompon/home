using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Home;


public partial class HomeGame : GameManager
{
	public static new HomeGame Current;

	public List<ChatCommandAttribute> ChatCommands { get; set; }

	private RemoteDb _db;

	public HomeGame()
	{
		Current = this;

		_db = new RemoteDb( "ws://localhost:8443/ws", null );

		// Load the game's different libraries
		LoadLibraries();

		if(Game.IsClient)
		{
			// Initialize HUD
			Game.RootPanel?.Delete(true);
			Game.RootPanel = new HomeHud();	
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		var player = new HomePlayer(client);
		player.Respawn();

		client.Pawn = player;

		// Load the player's data
		InventoryDbObject data = LoadPlayerData(client.SteamId);
		player.PlayerData = data;
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

	private InventoryDbObject LoadPlayerData(long steamId)
	{
		var query = _db.Query<InventoryDbObject>($"SteamId = {steamId}").Result;
		if(query == null) return new InventoryDbObject(steamId);
		
		InventoryDbObject data = query.FirstOrDefault(null as InventoryDbObject);

		// If none exists, create a new one
		if(data == null)
		{
			data = _db.Upsert(new InventoryDbObject(steamId)).Result;
		}

		// Increase times played
		data.TimesPlayed += 1L;
		data.Money += 100L;
		data = _db.Upsert(data).Result; // Result is necessary to ensure it's been updated server-side

		return data;
	}

}
