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

    private void InitRole(IClient clint)
    {
        Log.Info("🏠: Initializing player role...");
        switch(clint.SteamId)
        {
            // Owner
            case 76561198031113835: // Carson
                IsOwner = true;
                IsAdmin = true;
                IsModerator = true;
                break;
            
            // Moderators
            case 76561198038564197: // Yukon
            case 76561198043458654: // Del Pickle
            case 76561198037527559: // Stella Wisps
            case 76561198160401653: // Milk Man
                IsModerator = true;
                break;
        }
    }

    private async void LoadBadges()
    {
        if(IsOwner) GiveBadge("owner");
        if(IsAdmin) GiveBadge("admin");
        if(IsModerator) GiveBadge("moderator");

        switch(Client.SteamId)
        {
            // Developers/Contributors
            case 76561198031113835: // Carson
            case 76561197990720321: // ShadowBrain
            case 76561198155010327: // Luke
            case 76561197972285500: // ItsRifter
            case 76561198048910256: // Sugma Gaming
                GiveBadge("developer");
                break;
        }

        Log.Info("🏠: Awaiting game package...");
        var package = await Package.FetchAsync("carsonk.home", false);
        if(package != null)
        {
            Log.Info(package.Usage.Total.Seconds);
            // More than 30 days of playtime
            if(package.Interaction.Seconds > 2592000)
            {
                GiveBadge("30d");
            }
            // More than 7 days of playtime
            else if(package.Interaction.Seconds > 604800)
            {
                GiveBadge("7d");
            }
            // More than 24 hours of playtime
            else if(package.Interaction.Seconds > 86400)
            {
                GiveBadge("24h");
            }
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