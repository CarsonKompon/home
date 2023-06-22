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
    public BottomDirection Bottom { get; set; } = BottomDirection.ZNegative;

    [ResourceType("vmdl")]
    public string Model { get; set; }

    public string ClassName { get; set; } = "";

    public string EntityPackage { get; set; } = "";

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
    [HideInEditor] public Vector3 OriginOffset
    {
        get
        {
            if(!OriginLoaded)
            {
                FindOriginOffset();
            }
            return _OriginOffset;
        }
        set
        {
            _OriginOffset = value;
        }
    }
    [HideInEditor] private Vector3 _OriginOffset;
    [HideInEditor] private bool OriginLoaded = false;

    private Package LoadedPackage;

    public string GetModel() {
        if(!string.IsNullOrEmpty(RealModel)) return RealModel;
        return Model;
    }


    public static List<HomePlaceable> All => ResourceLibrary.GetAll<HomePlaceable>().ToList();

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

    private void FindOriginOffset()
    {
        OriginOffset = Vector3.Zero;
        Model model = Sandbox.Model.Load(Model);
        switch(Bottom)
        {
            case BottomDirection.ZNegative:
                OriginOffset = Vector3.Zero.WithZ(model.Bounds.Mins.z);
                break;
            case BottomDirection.ZPositive:
                OriginOffset = Vector3.Zero.WithZ(model.Bounds.Maxs.z);
                break;
            case BottomDirection.XNegative:
                OriginOffset = Vector3.Zero.WithX(model.Bounds.Mins.x);
                break;
            case BottomDirection.XPositive:
                OriginOffset = Vector3.Zero.WithX(model.Bounds.Maxs.x);
                break;
            case BottomDirection.YNegative:
                OriginOffset = Vector3.Zero.WithY(model.Bounds.Mins.y);
                break;
            case BottomDirection.YPositive:
                OriginOffset = Vector3.Zero.WithY(model.Bounds.Maxs.y);
                break;
        }
    }

    public enum BottomDirection
    {
        XPositive,
        XNegative,
        YPositive,
        YNegative,
        ZPositive,
        ZNegative
    }
}
