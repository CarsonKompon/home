using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged "construction".
/// </summary>
[Library( "home_npc_shop_construction" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Construction NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopConstructionNPC : BaseNPC
{

    public override string DisplayName => "#shop.construction";
    protected override string ClothingString => "[{\"id\":626961084},{\"id\":502735166},{\"id\":-620074038},{\"id\":-942608774},{\"id\":1045421669},{\"id\":1034075642},{\"id\":-477049300},{\"id\":-1441449628},{\"id\":-1492226718}]";

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), "construction", "#shop.construction");

        return false;
    }

}