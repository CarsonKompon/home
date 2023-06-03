using System.Linq;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Home;

public enum RoomState
{
    Vacant,
    Open,
    Locked,
    FriendsOnly
} 

public partial class RoomController : Entity
{
    public static new List<RoomController> All = new List<RoomController>();
    public static bool HasVacancies => All.Find(room => room.State == RoomState.Vacant) != null;

    [Net] public int Id { get; set; } = 0;

    public List<RoomBuildingZone> BuildingZones { get; set; } = null;
    public List<RoomEditableMaterial> EditableMaterials { get; set; } = null;
    [Net] public RoomState State { get; set; } = RoomState.Vacant;

    [Net] public IList<Entity> Entities { get; set; } = new List<Entity>();
    [Net] public HomePlayer RoomOwner { get; set; } = null;
    private RealTimeSince LastUpdate = 0f;

    public RoomController()
    {
        BuildingZones = new List<RoomBuildingZone>();
        EditableMaterials = new List<RoomEditableMaterial>();
        Event.Register(this);

        All.Add(this);
    }

    public RoomController(int id) : this()
    {
        Id = id;

        Log.Info("🏠: Initializing room #" + Id.ToString());
    }

    ~RoomController()
    {
        All.Remove(this);
        Event.Unregister(this);
    }

    [GameEvent.Tick.Server]
    public void OnTick()
    {
        if(LastUpdate > 2f)
        {
            // Check if the room owner still exists
            if(State != RoomState.Vacant && (RoomOwner == null || !RoomOwner.IsValid()))
            {
                RemoveOwner();
            }

            LastUpdate = 0f;
        }
    }

    public static RoomController GetOpenRoom()
    {
        if(!HasVacancies) return null;

        Random random = new Random();

        var vacants = All.Where(room => room.State == RoomState.Vacant).ToList();

        // Return a random room from the list
        return vacants[random.Next(0, vacants.Count)];
    }

    public RoomFrontDoor GetFrontDoor()
    {
        Log.Info(Id);
        return Entity.All.OfType<RoomFrontDoor>().FirstOrDefault(door => door.RoomId == Id);
    }

    public void SetOwner(HomePlayer owner)
    {
        if(RoomOwner != null) return;

        RoomOwner = owner;
        RoomOwner.Room = this;

        SetState(RoomState.Open);
        ResetName();

        // TODO: Spawn the owner's props here
    }

    public void RemoveOwner()
    {
        ResetName();
        if(RoomOwner != null) RoomOwner.Room = null;
        RoomOwner = null;

        SetState(RoomState.Vacant);

        // Delete all the props
        foreach(var ent in Entities)
        {
            ent.Delete();
        }
        Entities.Clear();
    }

    public void SetState(RoomState state)
    {
        State = state;
        GetFrontDoor()?.SetState(state);
    }

    public bool PointInside(Vector3 position)
    {
        for(int i=0; i<BuildingZones.Count; i++)
        {
            // Check if position is without min and max bounds
            var bounds = BuildingZones[i].PhysicsBody.GetBounds();
            if(position.x < bounds.Mins.x || position.y < bounds.Mins.y || position.z < bounds.Mins.z) continue;
            if(position.x > bounds.Maxs.x || position.y > bounds.Maxs.y || position.z > bounds.Maxs.z) continue;
            return true;
        }
        return false;
    }

    public void ResetName()
    {
        Name = "Room #" + Id.ToString();
    }

    public RoomLayout SaveLayout(string name = "New Layout")
    {
        RoomLayout layout = new RoomLayout();
        layout.Name = Name;

        foreach(var ent in Entities)
        {
            if(ent == null || !ent.IsValid()) continue;
            if(ent.Components.Get<PlaceableComponent>() is not PlaceableComponent component) continue;
            RoomLayoutEntry entry = new RoomLayoutEntry();
            Transform localTransform = GetFrontDoor().StartTransform.ToLocal(ent.Transform);
            entry.Id = component.PlaceableId;
            entry.Position = localTransform.Position;
            entry.Rotation = localTransform.Rotation;
            entry.Scale = ent.Scale;
            entry.HasPhysics = component.HasPhysics;

            layout.Entries.Add(entry);
        }

        Log.Info(layout);

        return layout;
    }




    [ConCmd.Server("home_load_layout")]
    public static async void LoadLayout()
    {
        if(ConsoleSystem.Caller == null) return;
        if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;
        if(player.Room == null) return;
        
        RoomLayout layout = JsonSerializer.Deserialize<RoomLayout>(player.Client.GetClientData<string>("HomeUploadData"));

        // Change room name
        player.Room.Name = layout.Name;

        // Delete all the entities
        foreach(var ent in player.Room.Entities)
        {
            ent.Delete();
        }

        // Create new entities
        foreach(var entry in layout.Entries)
        {
            if(!player.UsePlaceable(entry.Id)) continue;
            Transform localTransform = player.Room.GetFrontDoor().StartTransform.ToWorld(new Transform(entry.Position, entry.Rotation));
            Entity ent = await HomeGame.SpawnPlaceable(entry.Id, player.Client.SteamId, localTransform.Position, localTransform.Rotation, entry.Scale);
            HomeGame.SetPlaceablePhysics(ent.NetworkIdent, entry.HasPhysics);
        }
    }

    [ConCmd.Server("home_rename_room")]
    public static void Rename(string newName)
    {
        if(ConsoleSystem.Caller == null) return;
        if(ConsoleSystem.Caller.Pawn is not HomePlayer player) return;
        if(player.Room == null) return;

        player.Room.Name = newName;
    }

}