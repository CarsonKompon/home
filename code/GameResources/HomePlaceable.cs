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
    PackageEntity,
    PackageProp
}

public enum PlaceableState
{
    Visible,
    Hidden,
    Disabled
}

[GameResource("Home Placeable", "placeabl", "Describes a placeable model or entity that can be stored in the player's inventory and placed in their room.", Icon = "house" )]
public partial class HomePlaceable : GameResource, IShopItem
{
    public string Name { get; set; } = "Missingname.";
    public string Description { get; set; } = "";

    public int Cost { get; set; } = 0;

    public PlaceableState State { get; set; } = PlaceableState.Visible;

    public string[] Categories { get; set; } = { "furniture" };

    public PlaceableType Type { get; set; } = PlaceableType.Prop;
    public PhysicsMotionType PhysicsType { get; set; } = PhysicsMotionType.Keyframed;
    public BottomDirection Bottom { get; set; } = BottomDirection.ZNegative;

    [ResourceType("vmdl")]
    public string Model { get; set; }

    public string ClassName { get; set; } = "";

    public string CloudIdent { get; set; } = "";
    public string CloudModel { get; set; } = "";

    [ResourceType("png")]
    public string ThumbnailOverride { get; set; } = "";

    [Category("Sounds")]
    [ResourceType("vsnd")]
    public string PlaceSound { get; set; }

    [Category("Sounds")]
    [ResourceType("vsnd")]
    public string StashSound { get; set; }



    private string PackageThumbnail = "";
    [HideInEditor] public Texture Texture;
    [HideInEditor] public Transform TransformOffset
    {
        get
        {
            if(!TransformOffsetLoaded)
            {
                FindTransformOffset();
            }
            return _TransformOffset;
        }
    }
    [HideInEditor] private Transform _TransformOffset;
    [HideInEditor] private bool TransformOffsetLoaded = false;

    private Package LoadedPackage;

    public static List<HomePlaceable> All => ResourceLibrary.GetAll<HomePlaceable>().ToList();

    public static HomePlaceable Find(string id)
    {
        return All.Find(p => p.ResourceName == id);
    }

    public static HomePlaceable FindByModel(string model)
    {
        return All.Find(p => p.Model == model);
    }

    public static HomePlaceable FindByName(string name)
    {
        return All.Find(p => p.Name == name);
    }

    public static HomePlaceable FindByCost(int cost)
    {
        return All.Find(p => p.Cost <= cost);
    }

    public async Task<Texture> GetThumbnail()
    {
        if(string.IsNullOrEmpty(ThumbnailOverride))
        {
            if(!string.IsNullOrEmpty(CloudIdent))
            {
                return await PackageHelper.GetThumbnail(CloudIdent);

            }
            return SceneHelper.CreateModelThumbnail(Model);
        }
        
        return Texture.Load(FileSystem.Mounted, ThumbnailOverride);
    }

    public string _VideoThumbnail { get; set; }
    public async Task<string> GetVideoThumbnail()
    {
        if(string.IsNullOrEmpty(_VideoThumbnail))
        {
            if(!string.IsNullOrEmpty(CloudIdent))
            {
                return await PackageHelper.GetVideoThumbnail(CloudIdent);
            }
            return "";
        }
        return _VideoThumbnail;
    }

    private void FindTransformOffset()
    {
        _TransformOffset = Transform.Zero;
        if(string.IsNullOrEmpty(Model)) return;
        Model model = Sandbox.Model.Load(Model);
        switch(Bottom)
        {
            case BottomDirection.ZNegative:
                _TransformOffset.Position = Vector3.Zero.WithZ(model.Bounds.Mins.z);
                _TransformOffset.Rotation = Rotation.LookAt(Vector3.Forward, Vector3.Up);
                break;
            case BottomDirection.ZPositive:
                _TransformOffset.Position = Vector3.Zero.WithZ(model.Bounds.Maxs.z);
                _TransformOffset.Rotation = Rotation.LookAt(Vector3.Backward, Vector3.Up);
                break;
            case BottomDirection.XNegative:
                _TransformOffset.Position = Vector3.Zero.WithX(model.Bounds.Mins.x);
                _TransformOffset.Rotation = Rotation.LookAt(Vector3.Left, Vector3.Up);
                break;
            case BottomDirection.XPositive:
                _TransformOffset.Position = Vector3.Zero.WithX(model.Bounds.Maxs.x);
                _TransformOffset.Rotation = Rotation.LookAt(Vector3.Right, Vector3.Up);
                break;
            case BottomDirection.YNegative:
                _TransformOffset.Position = Vector3.Zero.WithY(model.Bounds.Mins.y);
                _TransformOffset.Rotation = Rotation.LookAt(Vector3.Down, Vector3.Up);
                break;
            case BottomDirection.YPositive:
                _TransformOffset.Position = Vector3.Zero.WithY(model.Bounds.Maxs.y);
                _TransformOffset.Rotation = Rotation.LookAt(Vector3.Up, Vector3.Up);
                break;
        }
        Log.Info(_TransformOffset.Position.ToString());
        TransformOffsetLoaded = true;
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
