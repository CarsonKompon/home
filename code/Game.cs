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

	public static List<RoomLayout> LocalLayouts = new List<RoomLayout>();
	public List<ChatCommandAttribute> ChatCommands { get; set; }

	public HomeGame()
	{
		Current = this;

		// Load the game's different libraries
		LoadLibraries();

		if(Game.IsClient)
		{
			// Initialize HUD
			Game.RootPanel?.Delete(true);
			Game.RootPanel = new HomeHud();

			// // Load local layouts
			// long steamId = Game.LocalClient.SteamId;
			// if(!FileSystem.Data.DirectoryExists(steamId.ToString()))
			// {
			// 	FileSystem.Data.CreateDirectory(steamId.ToString());
			// }
			// if(!FileSystem.Data.DirectoryExists(steamId + "/layouts"))
			// {
			// 	FileSystem.Data.CreateDirectory(steamId + "/layouts");
			// }
			// foreach(string file in FileSystem.Data.FindFile(steamId + "/layouts", "*.json"))
			// {
			// 	RoomLayout layout = FileSystem.Data.ReadJson<RoomLayout>(file);
			// 	LocalLayouts.Add(layout);
			// }
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );
		var player = new HomePlayer(client);
		player.Respawn();

		client.Pawn = player;

		player.LoadPlayerDataClientRpc(To.Single(client));
	}

	public override void OnVoicePlayed( IClient cl )
	{
		HomeVoiceList.Current?.OnVoicePlayed( cl.SteamId, cl.Voice.CurrentLevel );
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

	[ConCmd.Server("home_try_place")]
	public static void TryPlace()
	{
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;
		if(player.Placing == "") return;
		if(player.Room == null) return;

		// Check the placeable
		HomePlaceable placeable = HomePlaceable.Find(player.Placing);
		if(placeable == null) return;

		// Check placing in the room
		if(player.Room.PointInside(player.PlacingPosition) == false) return;

		// Check if we are moving a prop or placing a new one
		if(player.MovingEntity != null)
		{
			// Move the prop
			RoomProp prop = player.MovingEntity as RoomProp;
			prop.Position = player.PlacingPosition;
			prop.Rotation = player.PlacingRotation;
			prop.LocalAngle = player.PlacingAngle;
		}
		else
		{
			// Check the player's inventory
			if(!player.UsePlaceable(placeable.Id)) return;

			// Create the prop
			RoomProp prop = new RoomProp(placeable, ConsoleSystem.Caller.SteamId)
			{
				Position = player.PlacingPosition,
				Rotation = player.PlacingRotation,
				LocalAngle = player.PlacingAngle
			};

			// Add the prop to the room
			player.Room.Props.Add(prop);
		}

		player.FinishPlacing();
	}

	[ConCmd.Server("home_try_pickup")]
	public static void TryPickup()
	{
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;
		if(player.MovingEntity == null) return;

		//Check the prop
		if(player.MovingEntity is not RoomProp prop) return;
		if(!player.CanUsePlaceable(prop.PlaceableId)) return;
		player.UnusePlaceable(prop.PlaceableId);

		// Remove the prop from the room
		prop.Delete();

		player.FinishPlacing();
	}

	[ConCmd.Admin("home_give_placeable", Help = "Gives a placeable to a player")]
	public static void GivePlaceable(string id)
	{
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;

		// Check the placeable
		HomePlaceable placeable = HomePlaceable.Find(id);
		if(placeable == null) return;

		// Give the placeable to the player
		player.GivePlaceable(placeable.Id);
	}

}
