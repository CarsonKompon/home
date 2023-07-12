using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;

namespace Home;

[Library("home_leaderboard_screen"), HammerEntity]
[Title("Leaderboard Screen"), Category("Other"), Icon("aod")]
public class LeaderboardPanelEntity : Entity
{
    [Property("leaderboard_name"), Title("Leaderboard Name")]
    public string LeaderboardName { get; set; } = "Tetros Leaderboard";

    [Property("leaderboard_stats"), Title("Leaderboard Stats")]
    public string LeaderboardStats { get; set; } = "tetros_highscore";

    private LeaderboardPanel Screen { get; set; }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        Screen = new LeaderboardPanel();
        Screen.Transform = Transform;
        Screen.Position += Vector3.Up * 60f;

        InitScreen();
    }

    private void InitScreen()
    {
        var stats = LeaderboardStats.Split(",").ToArray();
        Screen.Init(LeaderboardName, stats);
    }

}