using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged "foliage".
/// </summary>
[Library( "home_npc_shop_furniture" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Furniture NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopFurnitureNPC : BaseNPC
{

    public override string DisplayName => "#shop.furniture";
    protected override string ClothingString => "[{\"id\":-1266562526},{\"id\":-1901789076},{\"id\":56388154},{\"id\":1977425295},{\"id\":791197114},{\"id\":626961084},{\"id\":1254089650},{\"id\":915965493},{\"id\":591027714}]";

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), "furniture", "#shop.furniture");

        return false;
    }

}