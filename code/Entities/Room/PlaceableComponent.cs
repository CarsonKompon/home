using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Home;


public partial class PlaceableComponent : EntityComponent
{
    [Net] public long OwnerId { get; set; }
    [Net] public string PlaceableId { get; set; }
    [Net] public float LocalAngle { get; set; } = 0f;
    [Net] public bool HasPhysics { get; set; } = false;

    public HomePlaceable Placeable;

    public PlaceableComponent()
    {
    }

    public PlaceableComponent(HomePlaceable placeable, long owner)
    {
        PlaceableId = placeable.Id;
        OwnerId = owner;

        Placeable = HomePlaceable.Find(PlaceableId);
        HasPhysics = Placeable.PhysicsType == PhysicsMotionType.Dynamic;
    }

    protected override void OnActivate( )
    {
        if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
        {
            Log.Info("what");
            StashEntry entry = player.Stash.FirstOrDefault(x => x.Id == PlaceableId);
            entry.Used++;
        }

        base.OnActivate();
    }

    protected override void OnDeactivate( ) 
    {
        if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
        {
            StashEntry entry = player.Stash.FirstOrDefault(x => x.Id == PlaceableId);
            entry.Used--;
        }

        base.OnDeactivate();
    }

    // public override void Spawn()
    // {
    //     base.Spawn();
    //     SetupPhysicsFromModel(PhysicsMotionType.Static);

    //     if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
    //     {
    //         StashEntry entry = player.Stash.FirstOrDefault(x => x.Id == PlaceableId);
    //         entry.Used++;
    //     }
    // }

	// protected override void OnDestroy()
	// {
    //     if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
    //     {
    //         StashEntry entry = player.Stash.FirstOrDefault(x => x.Id == PlaceableId);
    //         entry.Used--;
    //     }

	// 	base.OnDestroy();
	// }
}