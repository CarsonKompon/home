using System.Collections.Generic;
using System;
using Sandbox;


[GameResource("Home Placeable", "placeabl", "Describes a placeable model or entity that can be stored in the player's inventory and placed in their room.", Icon = "house" )]
public partial class HomePlaceable : GameResource
{
    public string Id { get; set; } = "missing_id";
    public string Name { get; set; } = "Missingname.";

    public int Cost { get; set; } = 0;

    [ResourceType("vmdl")]
    public string Model { get; set; }

    [Category("Sounds")]
    [ResourceType("vsnd")]
    public string PlaceSound { get; set; }

    [Category("Sounds")]
    [ResourceType("vsnd")]
    public string StashSound { get; set; }



    public static IReadOnlyList<HomePlaceable> All => _all;
    internal static List<HomePlaceable> _all = new();

	protected override void PostLoad()
	{
		base.PostLoad();

        if(!_all.Contains(this))
            _all.Add(this);
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
