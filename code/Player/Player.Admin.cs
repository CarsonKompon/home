using System;
using System.Collections.Generic;
using Sandbox;

namespace Home;

public partial class HomePlayer
{
    [Net] public bool IsOwner {get; set;} = false;
    [Net] public bool IsAdmin {get; set;} = false;
    [Net] public bool IsModerator {get; set;} = false;

    public IDictionary<long, float> PlayerVolumes {get; set;} = new Dictionary<long, float>();


    private void InitAdmin(IClient client)
    {
        Game.AssertServer();

        InitRole(client);

        LoadAdminRpc(To.Single(client));
    }

    [ClientRpc]
    private void LoadAdminRpc()
    {
        Game.AssertClient();

        // Load Player Volumes
        foreach(var player in Game.Clients)
        {
            if(player.SteamId == Client.SteamId) continue;

            var voice = Cookie.Get<float>("home.voice." + player.SteamId.ToString(), 1);
            if(voice != 1.0f)
            {
                PlayerVolumes[player.SteamId] = voice;
            }
        }
    }

    private void InitRole(IClient client)
    {
        switch(client.SteamId)
        {
            // Carson
            case 76561198031113835:
                IsOwner = true;
                IsAdmin = true;
                IsModerator = true;
                break;
        }
    }

    public void TogglePlayerMute(long steamId)
    {
        Game.AssertClient();
        if(PlayerVolumes.ContainsKey(steamId) && PlayerVolumes[steamId] == 0)
        {
            PlayerVolumes[steamId] = 1;
        }
        else
        {
            PlayerVolumes[steamId] = 0;
        }
        Cookie.Set("home.voice." + steamId.ToString(), PlayerVolumes[steamId]);
    }

    public bool HasAdminPermissions()
    {
        return IsAdmin;
    }

    public bool HasModeratorPermissions()
    {
        return IsAdmin || IsModerator || (ConsoleSystem.GetValue("sv_cheats") == "1");
    }

    public string GetDisplayStyle()
    {
        if(IsOwner) return "rainbow";
        if(Client.SteamId == Game.LocalClient.SteamId) return "me";
        if(IsAdmin) return "admin";
        if(IsModerator) return "moderator";
        return "";
    }

}