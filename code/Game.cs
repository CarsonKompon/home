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
		// if(cl.Pawn is HomePlayer player)
		// {
		// 	player.OnVoicePlayed(cl.Voice.CurrentLevel);
		// }
	}

	[Event.Hotload]
	public static void LoadLibraries()
	{
		// Load the chat commands
		if(Current != null)
		{
			Current.ChatCommands = new List<ChatCommandAttribute>();
			foreach(TypeDescription typeDesc in TypeLibrary.GetTypes<ChatCommandAttribute>())
			{
				ChatCommandAttribute command = TypeLibrary.Create<ChatCommandAttribute>(typeDesc.TargetType);
				Current.ChatCommands.Add(command);
			}
		}
	}

	[GameEvent.Tick.Client]
	private void ClientTick()
	{
		CheckClothingQueue();
	}

	[ConCmd.Server("home_try_place")]
	public static async void TryPlace()
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

		// Check if we are moving an entity or placing a new one
		if(player.MovingEntity != null)
		{
			// Move the entity
			if(player.MovingEntity.Components.Get<PlaceableComponent>() is PlaceableComponent component)
			{
				component.SetPhysicsType(PhysicsMotionType.Keyframed);

				player.MovingEntity.Position = player.PlacingPosition;
				player.MovingEntity.Rotation = player.PlacingRotation;
				component.LocalAngle = player.PlacingAngle;

				component.SetPhysicsType(component.HasPhysics ? PhysicsMotionType.Dynamic : PhysicsMotionType.Keyframed);
				//player.MovingEntity.ResetInterpolation();
				player.MovingEntity.RenderDirty();
			}
		}
		else
		{
			// Check the player's inventory
			if(!player.UsePlaceable(placeable.Id)) return;

			// Create the placeable
			Entity ent = await SpawnPlaceable(placeable.Id, ConsoleSystem.Caller.SteamId, player.PlacingPosition, player.PlacingRotation, 1.0f);
			if(ent.Components.Get<PlaceableComponent>() is PlaceableComponent component)
			{
				component.LocalAngle = player.PlacingAngle;
			}
		}

		player.FinishPlacing();
	}

	[ConCmd.Server("home_try_pickup")]
	public static void TryPickup()
	{
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;
		if(player.MovingEntity == null) return;

		//Check if entity is a placeable
		if(player.MovingEntity.Components.Get<PlaceableComponent>() is not PlaceableComponent component) return;
		player.UnusePlaceable(component.PlaceableId);

		// Remove the prop from the room
		player.MovingEntity.Delete();

		player.FinishPlacing();
	}

	[ConCmd.Server]
	public static void PurchasePlaceable(string id)
	{
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;

		// Check the placeable
		HomePlaceable placeable = HomePlaceable.Find(id);
		if(placeable == null) return;

		// Check the player's money
		if(!player.HasMoney(placeable.Cost)) return;
		player.TakeMoney(placeable.Cost);

		// Give the placeable to the player
		player.GivePlaceable(placeable.Id);
	}

	[ConCmd.Server]
	public static void PurchaseClothing(int id)
	{
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;

		// Check the clothing
		HomeClothing clothing = HomeClothing.AllHome.Where(x => x.ResourceId == id).FirstOrDefault();
		if(clothing == null) return;

		// Check the player's money
		if(!player.HasMoney(clothing.Cost)) return;
		player.TakeMoney(clothing.Cost);

		// Give the placeable to the player
		player.GiveClothing(clothing.ResourceId);
	}

	[ConCmd.Admin("home_give_money", Help = "Gives money to a player")]
	public static void GiveMoney(int amount, string target = "")
	{
		if(target != "")
		{
			Game.Clients.Where(x => x.Name.ToLower().Contains(target.ToLower())).ToList().ForEach(x => GiveMoney(amount, x.SteamId.ToString()));
			return;
		}

		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;

		// Give the money to the player
		player.GiveMoney(amount);
	}

	[ConCmd.Admin("home_give_placeable", Help = "Gives a placeable to a player")]
	public static void GivePlaceable(string id, string target = "")
	{
		if(target != "")
		{
			Game.Clients.Where(x => x.Name.ToLower().Contains(target.ToLower())).ToList().ForEach(x => GivePlaceable(id, x.SteamId.ToString()));
			return;
		}
		
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;

		// Check the placeable
		HomePlaceable placeable = HomePlaceable.Find(id);
		if(placeable == null) return;

		// Give the placeable to the player
		player.GivePlaceable(placeable.Id);
	}

	[ConCmd.Server]
	public static void GiveClothing(int id)
	{		
		// Check the player and their variables
		if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;

		// Check the placeable
		HomeClothing clothing = HomeClothing.AllHome.Where(x => x.ResourceId == id).FirstOrDefault();
		if(clothing == null) return;

		// Give the placeable to the player
		player.GiveClothing(clothing.ResourceId);
	}

	public static async Task<Entity> SpawnPlaceable(string id, long owner, Vector3 position, Rotation rotation, float scale)
	{
		HomePlaceable placeable = HomePlaceable.Find(id);
		HomePlayer player = Game.Clients.Where(x => x.SteamId == owner).FirstOrDefault()?.Pawn as HomePlayer;
		if(player == null) return null;

		switch(placeable.Type)
        {
            case PlaceableType.Prop:

				// Spawn the prop
                Prop prop = new Prop()
				{
					Position = position,
					Rotation = rotation,
					Scale = scale,
					Transmit = TransmitType.Always
				};

				// Set the model and physics
				prop.SetModel(placeable.Model);
				
				// Add the component
				PlaceableComponent component = new PlaceableComponent(placeable, owner);
				prop.Components.Add(component);
				component.SetPhysicsType(placeable.PhysicsType);

				// Add the prop to the room
				player.Room.Entities.Add(prop);

                return prop;

            case PlaceableType.Entity:
                // Getting a type that matches the name
                var entityType = TypeLibrary.GetType<Entity>( placeable.ClassName )?.TargetType;
                if ( entityType == null ) return null;

                // Creating an instance of that type
                Entity entity = TypeLibrary.Create<Entity>( entityType );
				if(entity == null) return null;

				// Setting the entity's position
				entity.Position = position;
				entity.Rotation = rotation;
				entity.Scale = scale;
				entity.Transmit = TransmitType.Always;

				// Add the component
				component = new PlaceableComponent(placeable, owner);
				entity.Components.Add(component);
				component.SetPhysicsType(placeable.PhysicsType);

				// Add the entity to the room
				player.Room.Entities.Add(entity);

                return entity;

            case PlaceableType.PackageEntity:
                // Create the package entity
				Entity packageEntity = await SpawnPackage(placeable.CloudIdent, position, rotation, scale, placeable.ClassName);
				packageEntity.Transmit = TransmitType.Always;

				// Add the component
				component = new PlaceableComponent(placeable, owner);
				packageEntity.Components.Add(component);
				component.SetPhysicsType(placeable.PhysicsType);

				// Add the package entity to the room
				player.Room.Entities.Add(packageEntity);

                return packageEntity;
			case PlaceableType.PackageProp:
				// Create the package prop
				Prop packageProp = await SpawnPackageProp(placeable.CloudIdent, position, rotation, scale);
				packageProp.Transmit = TransmitType.Always;

				// Add the component
				component = new PlaceableComponent(placeable, owner);
				packageProp.Components.Add(component);
				component.SetPhysicsType(placeable.PhysicsType);

				// Add the package prop to the room
				player.Room.Entities.Add(packageProp);

				return packageProp;

        }

		return null;
	}

	public static async Task<Entity> SpawnPackage(string ident, Vector3 position, Rotation rotation, float scale, string className = "")
    {
        var package = await Package.FetchAsync( ident, false );
        if(className == "")
		{
			className = package.GetMeta( "PrimaryAsset", "" );
		}
		if(string.IsNullOrEmpty(className))
		{
			Log.Warning( $"PrimaryAsset wasn't found for {package.FullIdent}");
			return null;
		}

        var thing = await package.MountAsync( true );

		var type = TypeLibrary.GetType( className );
		if ( type == null )
		{
			Log.Warning( $"'{className}' type wasn't found for {package.FullIdent}" );
			return null;
		}

		var entity = type.Create<Entity>();
		entity.Position = position;
		entity.Rotation = rotation;
		entity.Scale = scale;

		return entity;
    }

	public static async Task<Prop> SpawnPackageProp(string ident, Vector3 position, Rotation rotation, float scale)
	{
		var package = await Package.FetchAsync( ident, false );
		if ( package == null || package.PackageType != Package.Type.Model || package.Revision == null )
		{
			// spawn error particles
			return null;
		}

		string modelName = package.GetMeta( "PrimaryAsset", "models/dev/error.vmdl" );
		var mins = package.GetMeta( "RenderMins", Vector3.Zero );
		var maxs = package.GetMeta( "RenderMaxs", Vector3.Zero );

		// downloads if not downloads, mounts if not mounted
		await package.MountAsync();

		var model = Model.Load(modelName);
		if(model == null || model.IsError) return null;

		var ent = new Prop
		{
			Position = position,
			Rotation = rotation,
			Scale = scale,
			Model = model
		};

		ent.SetupPhysicsFromModel(PhysicsMotionType.Keyframed);

		// If there's no physics model, create a simple OBB
		if ( !ent.PhysicsBody.IsValid() )
		{
			ent.SetupPhysicsFromOBB( PhysicsMotionType.Dynamic, ent.CollisionBounds.Mins, ent.CollisionBounds.Maxs );
		}

		return ent;
	}

	[ConVar.Server("home_set_placeable_physics")]
	public static void SetPlaceablePhysics(int networkIdent, bool enabled)
	{
		// Find the entity
		Entity entity = Entity.FindByIndex(networkIdent);
		if(entity == null) return;

		// Check if the entity has a placeable component
		if(entity.Components.Get<PlaceableComponent>() is not PlaceableComponent component) return;

		// Set the physics
		if(entity is ModelEntity model)
        {
            if(enabled)
            {
                component.SetPhysicsType(PhysicsMotionType.Dynamic);
            }
            else
            {
                component.SetPhysicsType(PhysicsMotionType.Keyframed);
            }
			component.HasPhysics = enabled;
        }
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


	// Load the clothing in the queue
	bool IsLoadingClothing = false;
	private void CheckClothingQueue()
	{
		if(HomeClothing.LoadingQueue.Count() > 0 && !IsLoadingClothing)
		{
			LoadNextClothing();
		}
	}

	private async void LoadNextClothing()
	{
		IsLoadingClothing = true;

		// Get the next clothing
		HomeClothing clothing = HomeClothing.LoadingQueue.First();
		HomeClothing.LoadingQueue.Remove(clothing);

		if(clothing.CloudModel != "")
		{
			Package pck = await Package.FetchAsync(clothing.CloudModel, false);
			await pck.MountAsync(true);
			clothing.Model = pck.GetMeta("PrimaryAsset", "");
		}

		IsLoadingClothing = false;
	}

}
