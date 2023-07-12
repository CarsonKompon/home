using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Sandbox;
using Home.Util;

namespace Home;

[GameResource("Home Badge", "badge", "Describes a Home Badge.", Icon = "verified" )]
public partial class HomeBadge : GameResource
{
    public string Name { get; set; } = "Missingname.";

    [ResourceType("png")]
    public string Icon { get; set; } = "";

    public bool RequiresAuthority { get; set; } = false;



    public static List<HomeBadge> All => ResourceLibrary.GetAll<HomeBadge>().ToList();

    public static HomeBadge FindById(string id)
    {
        return All.Find(p => p.ResourceName.Split("/").Last() == id);
    }

    public static HomeBadge FindByName(string name)
    {
        return All.Find(p => p.Name == name);
    }
}
