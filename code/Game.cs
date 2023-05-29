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
		if(player.RoomNumber == -1) return;

		// Check the placeable
		HomePlaceable placeable = HomePlaceable.Find(player.Placing);
		if(placeable == null) return;

		// Check placing in the room
		RoomController room = RoomController.All.Find(room => room.Id == player.RoomNumber);
		if(room == null) return;
		if(room.PointInside(player.PlacingPosition) == false) return;

		// Check the player's inventory
		if(!player.HasPlaceable(placeable.Id)) return;
		player.TakePlaceable(placeable.Id);

		// Create the prop
		RoomPropStatic prop = new RoomPropStatic(placeable.Model)
		{
			Position = player.PlacingPosition,
			Rotation = Rotation.From(0, player.PlacingRotation, 0)
		};

		// Add the prop to the room
		room.Props.Add(prop);
	}

}
