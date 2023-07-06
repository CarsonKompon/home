using Sandbox;
using Editor;
using Home.Util;

namespace Home;

/// <summary>
/// Talking to this NPC will test a crash.
/// </summary>
[Library( "home_npc_shop_clothing" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Clothing NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopClothingNPC : BaseNPC
{

    public override string DisplayName => "#shop.clothing";
    protected override string ClothingString => "[{\"id\":1594058106},{\"id\":-1191994301},{\"id\":-778804355},{\"id\":1356410853},{\"id\":-309418524},{\"id\":-2045103204},{\"id\":-655136478},{\"id\":829898412},{\"id\":533938816}]";

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopClothing.Open(To.Single(user), "#shop.clothing");

        return false;
    }

}