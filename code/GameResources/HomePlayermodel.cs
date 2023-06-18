using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Sandbox;
using Home.Util;

namespace Home;


[GameResource("Home Playermodel", "plymodel", "Describes a Home player mode.", Icon = "bungalow" )]
public partial class HomePlayermodel : GameResource
{
    public string Name { get; set; } = "Missingname.";

    public int Cost { get; set; } = 0;

    [ResourceType("vmdl")]
    public string Model { get; set; }

    [ResourceType("png")]
    public string ThumbnailOverride { get; set; } = "";

    [HideInEditor] public Texture Texture;

    private Package LoadedPackage;


    public static List<HomePlayermodel> All => ResourceLibrary.GetAll<HomePlayermodel>().ToList();

    public static HomePlayermodel Find(string name)
    {
        return All.Find(p => p.Name == name);
    }

    public static HomePlayermodel FindByCost(int cost)
    {
        return All.Find(p => p.Cost <= cost);
    }
}
