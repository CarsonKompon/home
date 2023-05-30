using Sandbox;
using System;
using System.Collections.Generic;

namespace Home;

public enum RoomState
{
    Vacant,
    Open,
    Locked,
    FriendsOnly
} 

public partial class RoomController : BaseNetworkable
{
    public static List<RoomController> All = new List<RoomController>();
    public static bool HasVacancies => All.Find(room => room.State == RoomState.Vacant) != null;

    public int Id { get; set; } = 0;

    public List<RoomBuildingZone> BuildingZones { get; set; } = null;
    public List<RoomEditableMaterial> EditableMaterials { get; set; } = null;
    public RoomFrontDoor FrontDoor { get; set; } = null;
    public RoomState State { get; set; } = RoomState.Vacant;

    public List<Entity> Props { get; set; } = new List<Entity>();

    [Net] public string Name { get; set; } = "N/A";
    [Net] public HomePlayer Owner { get; set; } = null;
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
            Log.Info("🏠: Room #" + Id.ToString() + ": " + Owner);
            // Check if the room owner still exists
            if(State != RoomState.Vacant && (Owner == null || !Owner.IsValid()))
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

        // for(int i=All.Count - 1; i>1; i--)
        // {
            
        // }

        // Return a random room from the list
        return All.Find(room => room.State == RoomState.Vacant);
    }

    public void SetOwner(HomePlayer owner)
    {
        if(Owner != null) return;

        Owner = owner;
        Owner.Room = this;

        SetState(RoomState.Open);
        ResetName();

        // TODO: Spawn the owner's props here
    }

    public void RemoveOwner()
    {
        ResetName();
        if(Owner != null) Owner.Room = null;
        Owner = null;

        SetState(RoomState.Vacant);

        // Delete all the props
        foreach(var prop in Props)
        {
            prop.Delete();
        }
        Props.Clear();
    }

    public void SetState(RoomState state)
    {
        State = state;
        FrontDoor?.SetState(state);
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

}