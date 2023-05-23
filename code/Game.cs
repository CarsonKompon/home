using System.Reflection;
using System.Data;
using System.Runtime.CompilerServices;
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Home;


public partial class HomeGame : GameManager
{
	public static new HomeGame Current;
	[Net] public IDictionary<long, HomeData> PlayerData { get; set; }

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
		LoadPlayerData(client);
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

	private void LoadPlayerData(IClient client)
	{
		if(!Game.IsServer) return;

		var query = _db.Query<HomeData>($"SteamId = {client.SteamId}").Result;
		HomeData data = query?.FirstOrDefault(null as HomeData);

		// If none exists, create a new one
		if(data == null)
		{
			data = new HomeData(client.SteamId);
			data.Save();
		}

		// Set the player
		data.SetPlayer(client.Pawn as HomePlayer);
		
		OnPlayerDataLoad(client.SteamId, data);

		PlayerData.Add(client.SteamId, data);
	}

	public static HomeData UploadPlayerData(HomeData data)
	{
		return Current._db.Upsert(data).Result; // Result is necessary to ensure it's been updated server-side
	}

	private void OnPlayerDataLoad(long steamId, HomeData data)
	{
		data.TimesPlayed += 1L;
		data.GiveMoney(50L);
		data.GivePlaceable(HomePlaceable.Find("chair_office_01"));

		data.Save();
	}

}
