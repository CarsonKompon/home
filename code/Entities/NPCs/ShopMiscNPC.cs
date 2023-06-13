using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged "misc".
/// </summary>
[Library( "home_npc_shop_misc" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Misc NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopMiscNPC : BaseNPC
{

    public override string DisplayName => "#shop.misc";
    protected override string ClothingString => "[{\"id\":1594058106},{\"id\":1772984322},{\"id\":-69855493},{\"id\":118689699},{\"id\":533403674},{\"id\":153578956},{\"id\":758978340},{\"id\":1753539681},{\"id\":-1611545262}]";
    public override void Spawn()
    {
        base.Spawn();
    }

    [GameEvent.Tick.Server]
	void Tick()
    {
        SetAnimParameter( "holdtype_pose", 4 );
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), "misc", "#shop.misc");

        return false;
    }

}