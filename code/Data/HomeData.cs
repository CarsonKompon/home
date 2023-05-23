using Sandbox;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Home;

public class HomeData : RemoteDb.DbObject
{
    public NumberLong SteamId { get; set; }
    public NumberLong TimesPlayed { get; set; }
    public NumberLong Money { get; set; }
    public List<HomeDataInventoryEntry> Inventory { get; set; }

    public HomeData()
    {
        TimesPlayed = 0;
        Money = 0;
        Inventory = new List<HomeDataInventoryEntry>();
    }

    public HomeData(long steamId) : this()
    {
        SteamId = steamId;
    }


    [JsonIgnore] public HomePlayer Player { get; protected set; }


    public bool Save()
    {
        if(!Game.IsServer) return false;

        if(HomeGame.OfflineMode)
        {
            Player.PlayerDataString = JsonSerializer.Serialize(this);
            HomeGame.SaveOfflineDataClientRpc(To.Single(Player.Client));
            return true;
        }
        
        return HomeGame.UploadPlayerData(this) == null;
    }

    public void SetPlayer(HomePlayer player)
    {
        if(!Game.IsServer) return;
        Player = player;
        UpdatePlayerVals();
    }

    private void UpdatePlayerVals()
    {
        if(!Game.IsServer) return;
        if(Player == null) return;
        Player.Money = Money;
    }


    public void GiveMoney(long amount)
    {
        if(!Game.IsServer) return;
        Money += amount;
        Player.Money = Money;
    }

    public bool HasMoney(long amount)
    {
        return Money >= amount;
    }

    public bool TakeMoney(long amount)
    {
        if(!Game.IsServer) return false;
        if(Money < amount) return false;
        Money = Money - amount;
        Player.Money = Money;
        return true;
    }

    public void GivePlaceable(HomePlaceable placeable, int amount = 1)
    {
        if(!Game.IsServer) return;
        if(placeable == null) return;
        HomeDataInventoryEntry entry = Inventory.Find(e => e.Id == placeable.Id);
        if(entry == null)
        {
            Inventory.Add(new HomeDataInventoryEntry(placeable.Id, amount));
        }
        else
        {
            entry.Amount += amount;
        }
    }
}

public class HomeDataInventoryEntry
{
    public string Id { get; set; }
    public int Amount { get; set; }

    public HomeDataInventoryEntry()
    {
        Id = "undefined";
        Amount = 0;
    }

    public HomeDataInventoryEntry(string id, int amount)
    {
        Id = id;
        Amount = amount;
    }
}