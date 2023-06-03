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


    private Package LoadedPackage;
    public string GetThumbnail() {
        if(ThumbnailOverride != "") return ThumbnailOverride;
        if(PackageIdent == "") return ThumbnailOverride;
        return LoadedPackage.Thumb;
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

        if(PackageIdent != "")
        {
            Package.Fetch(PackageIdent, false).ContinueWith((task) => {
                LoadedPackage = task.Result;
                ThumbnailOverride = LoadedPackage.Thumb;
            });
        }
	}

    public static HomePlaceable Find(string id)
    {
        return _all.Find(p => p.Id == id);
    }

    public static HomePlaceable FindByModel(string model)
    {
        return _all.Find(p => p.Model == model);
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
