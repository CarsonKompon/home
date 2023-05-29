using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Home;

public partial class RoomProp : Prop
{
    [Net] public long OwnerId { get; set; }
    [Net] public string PlaceableId { get; set; }

    public RoomProp()
    {
    }

    public RoomProp(HomePlaceable placeable, long owner)
    {
        PlaceableId = placeable.Id;
        OwnerId = owner;
        SetModel(placeable.Model);
    }

    public override void Spawn()
    {
        base.Spawn();
        SetupPhysicsFromModel(PhysicsMotionType.Static);

        if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
        {
            StashEntry entry = player.Stash.FirstOrDefault(x => x.Id == PlaceableId);
            entry.Used++;
        }
    }

	protected override void OnDestroy()
	{
        if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
        {
            StashEntry entry = player.Stash.FirstOrDefault(x => x.Id == PlaceableId);
            entry.Used--;
        }

		base.OnDestroy();
	}
}