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

public class RoomController
{
    public static List<RoomController> All = new List<RoomController>();
    public static bool HasVacancies => All.Find(room => room.State == RoomState.Vacant) != null;

    public int Id { get; set; } = 0;

    public List<RoomBuildingZone> BuildingZones { get; set; } = null;
    public List<RoomEditableMaterial> EditableMaterials { get; set; } = null;
    public RoomFrontDoor FrontDoor { get; set; } = null;
    public RoomState State { get; set; } = RoomState.Vacant;

    public HomePlayer Owner { get; set; } = null;

    public RoomController()
    {
        BuildingZones = new List<RoomBuildingZone>();
        EditableMaterials = new List<RoomEditableMaterial>();

        All.Add(this);
    }

    public RoomController(int id) : this()
    {
        Id = id;

        Log.Info("Initializing room #" + Id.ToString());
    }

    public static RoomController GetOpenRoom()
    {
        if(!HasVacancies) return null;

        Random random = new Random();

        for(int i=All.Count - 1; i>1; i--)
        {
            
        }

        // Return a random room from the list
        return All.Find(room => room.State == RoomState.Vacant);
    }

    public void SetOwner(HomePlayer owner)
    {
        if(Owner != null) return;

        Owner = owner;
        Owner.Room = this;

        SetState(RoomState.Open);

        // TODO: Spawn the owner's props here
    }

    public void RemoveOwner()
    {
        if(Owner == null) return;

        Owner.Room = null;
        Owner = null;

        SetState(RoomState.Vacant);

        // TODO: Remove the owner's props here
    }

    public void SetState(RoomState state)
    {
        State = state;
        FrontDoor?.SetState(state);
    }

}