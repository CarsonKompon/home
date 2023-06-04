using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Sandbox;

public enum PlaceableCategory
{
    Furniture,
    Building
}

public enum PlaceableType
{
    Prop,
    Entity,
    PackageEntity
}

public enum PlaceableState
{
    Visible,
    Hidden,
    Disabled
}

[GameResource("Home Placeable", "placeabl", "Describes a placeable model or entity that can be stored in the player's inventory and placed in their room.", Icon = "house" )]
public partial class HomePlaceable : GameResource
{
    public string Id { get; set; } = "missing_id";
    public string Name { get; set; } = "Missingname.";

    public int Cost { get; set; } = 0;

    public PlaceableState State { get; set; } = PlaceableState.Visible;

    public PlaceableCategory Category { get; set; } = PlaceableCategory.Furniture;

    public PlaceableType Type { get; set; } = PlaceableType.Prop;
    public PhysicsMotionType PhysicsType { get; set; } = PhysicsMotionType.Keyframed;

    [ResourceType("vmdl")]
    public string Model { get; set; }

    public string ClassName { get; set; } = "";

    public string PackageIdent { get; set; } = "";

    [ResourceType("png")]
    public string ThumbnailOverride { get; set; } = "";

    [Category("Sounds")]
    [ResourceType("vsnd")]
    public string PlaceSound { get; set; }

    [Category("Sounds")]
    [ResourceType("vsnd")]
    public string StashSound { get; set; }

    private string PackageThumbnail = "";
    [HideInEditor] public string RealModel = "";


    private Package LoadedPackage;
    public string GetThumbnail() {
        if(!string.IsNullOrEmpty(ThumbnailOverride)) return ThumbnailOverride;
        if(!string.IsNullOrEmpty(PackageIdent)) return PackageThumbnail;
        return LoadedPackage.Thumb;
    }

    public string GetModel() {
        if(!string.IsNullOrEmpty(RealModel)) return RealModel;
        return Model;
    }


    public static IReadOnlyList<HomePlaceable> All => _all;
    internal static List<HomePlaceable> _all = new();

	protected override void PostLoad()
	{
		base.PostLoad();

        if(!_all.Contains(this) && State != PlaceableState.Disabled)
        {
            _all.Add(this);
        }

        InitFromPackage();
	}

    public async void InitFromPackage()
    {
        if(string.IsNullOrEmpty(PackageIdent)) return;

        LoadedPackage = await Package.FetchAsync(PackageIdent, false);
        PackageThumbnail = LoadedPackage.Thumb;

        if(string.IsNullOrEmpty(Model))
        {
            var model = LoadedPackage.GetMeta("SingleAssetSource", "");
            if(!model.EndsWith(".vmdl")) model = LoadedPackage.GetMeta("PrimaryAsset", "");
            if(model.EndsWith(".vmdl"))
            {
                RealModel = model;
            }
        }

    
        // if(string.IsNullOrWhiteSpace(Model))
        // {
        //     var className = LoadedPackage.GetMeta( "PrimaryAsset", "" );
        //     if(string.IsNullOrEmpty(className)) return;
            
        //     await LoadedPackage.MountAsync( false );
            
        //     var entityType = TypeLibrary.GetType<Entity>( className )?.TargetType;
        //     if(entityType == null) return;
        //     var entity = TypeLibrary.Create<Entity>( entityType );
        //     if(entity == null) return;
        //     if(entity is ModelEntity modelEntity)
        //     {
        //         Model = modelEntity.GetModelName();
        //     }
        //     entity.Delete();
        // }
    }

    public static HomePlaceable Find(string id)
    {
        return _all.Find(p => p.Id == id);
    }

    public static HomePlaceable FindByModel(string model)
    {
        return _all.Find(p => p.GetModel() == model);
    }

    public static HomePlaceable FindByName(string name)
    {
        return _all.Find(p => p.Name == name);
    }

    public static HomePlaceable FindByCost(int cost)
    {
        return _all.Find(p => p.Cost <= cost);
    }
}
