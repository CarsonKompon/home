using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will allow players to check-in to an available room.
/// </summary>
[Library( "home_npc_shop_furnature" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Furnature NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopFurnatureNPC : BaseNPC
{

    public ShopFurnatureNPC()
    {
        ClothingString = "[{\"id\":-1266562526},{\"id\":-1901789076},{\"id\":56388154},{\"id\":1977425295},{\"id\":791197114},{\"id\":626961084},{\"id\":1254089650},{\"id\":915965493},{\"id\":591027714}]";
    }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        if (user is HomePlayer player)
        {
            player.GiveMoney(1000);
        }

        return false;
    }

}