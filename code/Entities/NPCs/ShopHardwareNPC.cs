namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged "hardware".
/// </summary>
[Library( "home_npc_shop_hardware" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Hardware NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopHardwareNPC : BaseNPC
{

    public override string DisplayName => "#shop.hardware";
    protected override string ClothingString => "[{\"id\":1772984322},{\"id\":1594058106},{\"id\":-778804355},{\"id\":1661366749},{\"id\":-1659713462},{\"id\":758978340},{\"id\":829898412},{\"id\":-309418524},{\"id\":-934412006}]";

    [GameEvent.Tick.Server]
	void Tick()
    {
        SetAnimParameter( "holdtype_pose", 2 );
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), "hardware", "#shop.hardware");

        return false;
    }

}
