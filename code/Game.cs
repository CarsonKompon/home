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

		HomeChatBox.Announce($"{client.Name} joined the server");

		player.LoadPlayerDataClientRpc(To.Single(client));
	}

	public override void ClientDisconnect( IClient client, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( client, reason );

		HomeChatBox.Announce($"{client.Name} left the server");
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

	[ClientRpc]
	public static void LoadLayout(string name)
	{
		Log.Info("loading clientrpc style");
		// Check the player and their variables
		if(!Game.IsClient) return;
		if(Game.LocalPawn is not HomePlayer player) return;
		if(player.Room == null) return;

		// Check the layout
		RoomLayout layout = player.RoomLayouts.First(l => l.Name == name);
		if(layout == null) return;

		// Load the layout
		Log.Info(layout);
		player.HomeUploadData = Json.Serialize(layout);
		ConsoleSystem.Run("home_load_layout");
	}

	[ClientRpc]
	public static void SaveLayout(string name, bool addNumber = false)
	{
		// Check the player and their variables
		if(!Game.IsClient) return;
		if(Game.LocalPawn is not HomePlayer player) return;
		if(player.Room == null) return;

		// TODO: Ask if player wants to overwrite layout if exists

		// Save the layout
		RoomLayout layout = player.Room.SaveLayout(name);

		// Add the layout to the local layouts
		if(player.RoomLayouts.Find(l => l.Name == name) == null)
		{
			player.RoomLayouts.Add(layout);
		}
		else
		{
			if(addNumber)
			{
				int number = 1;
				while(player.RoomLayouts.Find(l => l.Name == layout.Name) == null)
				{
					layout.Name = name + " (" + number + ")";
					number++;
				}
				player.RoomLayouts.Add(layout);
			}else{
				player.RoomLayouts[player.RoomLayouts.FindIndex(l => l.Name == layout.Name)] = layout;
			}
		}

		// Save the layout to a local file
		FileSystem.Data.WriteJson(player.Client.SteamId + "/layouts/" + layout.Name + ".json", layout);
	}

	[ClientRpc]
	public static void DeleteLayout(string name)
	{
		// Check the player and their variables
		if(!Game.IsClient) return;
		if(Game.LocalPawn is not HomePlayer player) return;
		if(player.Room == null) return;

		// Delete the layout file
		if(FileSystem.Data.FileExists(player.Client.SteamId + "/layouts/" + name + ".json"))
		{
			FileSystem.Data.DeleteFile(player.Client.SteamId + "/layouts/" + name + ".json");
		}

		// Delete the layout from the local layouts
		player.RoomLayouts.RemoveAll(l => l.Name == name);
	}

}
