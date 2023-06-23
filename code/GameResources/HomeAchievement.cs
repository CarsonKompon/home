using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Sandbox;
using Home.Util;

namespace Home;

public class AchievementProgress
{
    public string Name {get; set;}
    public int Progress {get; set;}
    public bool Unlocked {get; set;}
}

[GameResource("Home Achievement", "achieve", "Describes a Home achievement.", Icon = "emoji_events" )]
public partial class HomeAchievement : GameResource
{
    public string Name { get; set; } = "Missingname.";
    public int Goal { get; set; } = 1;

    public string[] Rewards { get; set; }

    [ResourceType("png")]
    public string Icon { get; set; } = "";


    public static List<HomeAchievement> All => ResourceLibrary.GetAll<HomeAchievement>().ToList();

    public static HomeAchievement Find(string name)
    {
        return All.Find(p => p.Name == name);
    }
}
