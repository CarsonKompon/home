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
	public static Dictionary<long, HomeData> PlayerData { get; set; } = new Dictionary<long, HomeData>();

	public List<ChatCommandAttribute> ChatCommands { get; set; }

	private RemoteDb _db;

	#region ConVars

		[ConVar.Server("home_sv_offline", Help = "Whether or not the server should run in offline mode")]
		private  static bool _offlineMode { get; set; }
		public static bool OfflineMode {
			get
			{
				if(!Current._db.Connected) return true;
				return _offlineMode;
			}
			set
			{
				_offlineMode = value;
			}
		}

	#endregion

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

		if(OfflineMode)
		{
			LoadOfflinePlayerDataClientRpc(To.Single(client));
			return;
		}

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


	[ClientRpc]
	public static void SaveOfflineDataClientRpc()
	{
		long steamId = Game.LocalClient.SteamId;
		string dataString = "";
		if(Game.LocalPawn is HomePlayer player)
		{
			dataString = player.PlayerDataString;
		}
		Log.Info("SAVING: " + dataString);
		Cookie.SetString("home." + (_offlineMode ? "offline" : "online") + "-data-." + steamId.ToString(), dataString);
	}

	// [ConCmd.Server( "home_save_offline_data" )]
	// public static void SaveOfflinePlayerData()
	// {
	// 	if(!PlayerData.ContainsKey(ConsoleSystem.Caller.SteamId))
	// 	{
	// 		Log.Info("NO DATA TO SAVE");
	// 		return;
	// 	}

	// 	string dataString = JsonSerializer.Serialize(PlayerData[ConsoleSystem.Caller.SteamId]);
	// 	if(ConsoleSystem.Caller.Pawn is HomePlayer player)
	// 	{
	// 		player.OfflinePlayerDataString = dataString;
	// 	}

	// }

	[ClientRpc]
	public static void LoadOfflinePlayerDataClientRpc()
	{
		long steamId = Game.LocalClient.SteamId;
		string dataString = Cookie.GetString("home." + (_offlineMode ? "offline" : "online") + "-data." + steamId.ToString(), "");
		if(dataString == "")
		{
			dataString = JsonSerializer.Serialize(new HomeData(steamId));
		}
		if(Game.LocalPawn is HomePlayer player)
		{
			player.ClientDataUpload = dataString;
		}
		Log.Info("LOADING: " + dataString);
		LoadOfflinePlayerData();
	}

	[ConCmd.Server( "home_load_offline_data" )]
	public static void LoadOfflinePlayerData()
	{
		long steamId = ConsoleSystem.Caller.SteamId;
		string dataString = "";
		if(ConsoleSystem.Caller.Pawn is HomePlayer player)
		{
			dataString = player.ClientDataUpload;
			Log.Info("OFFLINE LOADING: " + dataString);
			HomeData data;
			try
			{
				data = JsonSerializer.Deserialize<HomeData>(dataString);
			}
			catch(Exception e)
			{
				data = new HomeData(steamId);
			}
			data.SetPlayer(player);
			if(PlayerData.ContainsKey(steamId))
			{
				PlayerData[steamId] = data;
			}
			else
			{
				PlayerData.Add(steamId, data);
				Current.OnPlayerDataLoad(steamId, data);
			}
		}
	}

}
