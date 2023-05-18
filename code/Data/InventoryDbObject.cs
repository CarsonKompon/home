using Sandbox;

namespace Home;

public class InventoryDbObject : RemoteDb.DbObject
{
    public NumberLong SteamId { get; set; }
    public NumberLong TimesPlayed { get; set; }
    public NumberLong Money { get; set; }

    public InventoryDbObject()
    {
        TimesPlayed = 0;
        Money = 0;
    }

    public InventoryDbObject(long steamId) : this()
    {
        SteamId = steamId;
    }
}