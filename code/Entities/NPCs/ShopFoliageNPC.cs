using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged "foliage".
/// </summary>
[Library( "home_npc_shop_foliage" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Foliage NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopFoliageNPC : BaseNPC
{

    public override string DisplayName => "#shop.foliage";
    protected override string ClothingString => "[{\"id\":1594058106},{\"id\":1772984322},{\"id\":502735166},{\"id\":469696431},{\"id\":358536239},{\"id\":-2122754807},{\"id\":720795379},{\"id\":-1346233164},{\"id\":197300820},{\"id\":533938816}]";

    [GameEvent.Tick.Server]
	void Tick()
    {
        SetAnimParameter( "holdtype_pose", 2 );
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), "foliage", "#shop.foliage", "the_ruins");

        return false;
    }

}