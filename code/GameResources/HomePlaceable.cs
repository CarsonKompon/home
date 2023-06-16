using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Sandbox;
using Home.Util;

namespace Home;

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

    public string[] Categories { get; set; } = { "furniture" };

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
    [HideInEditor] public Texture Texture;

    private Package LoadedPackage;

    public string GetModel() {
        if(!string.IsNullOrEmpty(RealModel)) return RealModel;
        return Model;
    }


    public static List<HomePlaceable> All => ResourceLibrary.GetAll<HomePlaceable>().ToList();

	protected override void PostLoad()
	{
		base.PostLoad();

        // if(!_all.Contains(this) && State != PlaceableState.Disabled)
        // {
        //     _all.Add(this);
        // }

        InitFromPackage();
	}
    
    public async void InitFromPackage()
    {
        if(string.IsNullOrEmpty(PackageIdent)) return;

        LoadedPackage = await Package.FetchAsync(PackageIdent, false);
        LoadedPackage.IsMounted();
        //PackageThumbnail = LoadedPackage.Thumb;

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
        return All.Find(p => p.Id == id);
    }

    public static HomePlaceable FindByModel(string model)
    {
        return All.Find(p => p.GetModel() == model);
    }

    public static HomePlaceable FindByName(string name)
    {
        return All.Find(p => p.Name == name);
    }

    public static HomePlaceable FindByCost(int cost)
    {
        return All.Find(p => p.Cost <= cost);
    }
}
