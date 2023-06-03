using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Home;

public partial class RoomProp : Prop
{
    [Net] public long OwnerId { get; set; }
    [Net] public string PlaceableId { get; set; }
    [Net] public float LocalAngle { get; set; } = 0f;
    [Net] public Entity CustomEntity { get; set; }

    public bool HasCustomEntity => CustomEntity != null && CustomEntity.IsValid();

    public RoomProp()
    {
    }

    public RoomProp(HomePlaceable placeable, long owner)
    {
        PlaceableId = placeable.Id;
        OwnerId = owner;

        switch(placeable.Type)
        {
            case PlaceableType.Prop:
                SetModel(placeable.Model);
                break;
            case PlaceableType.Entity:
                // Getting a type that matches the name
                var entityType = TypeLibrary.GetType<Entity>( placeable.ClassName )?.TargetType;
                if ( entityType == null ) return;

                // Creating an instance of that type
                CustomEntity = TypeLibrary.Create<Entity>( entityType );
                break;
            case PlaceableType.PackageEntity:
                SpawnPackage(placeable.PackageIdent);
                break;
        }
    }

    public async void SpawnPackage(string ident)
    {
        var package = await Package.FetchAsync( ident, false );
        if(ClassName == "") ClassName = package.GetMeta( "PrimaryAsset", "" );
        if(string.IsNullOrEmpty(ClassName)) return;
        if(!(package.Tags.Contains("runtime") && package.PackageType == Package.Type.Addon)) return;
        
        var thing = await package.MountAsync( true );

		var type = TypeLibrary.GetType( ClassName );
		if ( type == null )
		{
			Log.Warning( $"'{ClassName}' type wasn't found for {package.FullIdent}" );
			return;
		}

		var ent = type.Create<Entity>();
		ent.Transform = Transform;
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