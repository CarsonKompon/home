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
        ShouldTransmit = true;
    }

    public PlaceableComponent(HomePlaceable placeable, long owner) : this()
    {
        PlaceableId = placeable.ResourceName;
        OwnerId = owner;

        Placeable = HomePlaceable.Find(PlaceableId);
        HasPhysics = Placeable.PhysicsType == PhysicsMotionType.Dynamic;
    }

    protected override void OnActivate( )
    {
        if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
        {
            StashEntry entry = player.Data.Stash.FirstOrDefault(x => x.Id == PlaceableId);
            entry.Used++;
        }

        base.OnActivate();
    }

    protected override void OnDeactivate( ) 
    {
        if(Game.IsServer && Game.Clients.FirstOrDefault(x => x.SteamId == OwnerId)?.Pawn is HomePlayer player)
        {
            StashEntry entry = player.Data.Stash.FirstOrDefault(x => x.Id == PlaceableId);
            if(entry.Used > 0) entry.Used--;
        }

        base.OnDeactivate();
    }

    public void SetPhysicsType(PhysicsMotionType type)
    {
        Game.AssertServer();

        if(Entity is ModelEntity model)
        {
            model.SetupPhysicsFromModel(type);
        }
    }

    private PhysicsBodyType MotionToBodyType(PhysicsMotionType type)
    {
        switch(type)
        {
            case PhysicsMotionType.Static:
                return PhysicsBodyType.Static;
            case PhysicsMotionType.Dynamic:
                return PhysicsBodyType.Dynamic;
            case PhysicsMotionType.Keyframed:
                return PhysicsBodyType.Keyframed;
            default:
                return PhysicsBodyType.Static;
        }
    }

    [ConCmd.Server]
    public static void SetColor(int ident, float r, float g, float b)
    {
        Game.AssertServer();

        if(Entity.FindByIndex(ident) is ModelEntity model)
        {
            model.RenderColor = new Color(r, g, b);
        }
    }

    public void Destroy()
    {
        Game.AssertServer();

        foreach(var child in Entity.Children)
        {
            if(child is HomePlayer player)
            {
                player.ResetController();
            }
            child.SetParent(null, null, Transform.Zero);
        }

        Entity.Delete();
    }

}