using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will allow players to check-in to an available room.
/// </summary>
[Library( "home_npc_shop_furniture" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Furniture NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopFurnitureNPC : BaseNPC
{

    public ShopFurnitureNPC()
    {
        DisplayName = "Furniture Shop";
        ClothingString = "[{\"id\":-1266562526},{\"id\":-1901789076},{\"id\":56388154},{\"id\":1977425295},{\"id\":791197114},{\"id\":626961084},{\"id\":1254089650},{\"id\":915965493},{\"id\":591027714}]";
    }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopFurniture.Open(To.Single(user));

        return false;
    }

}