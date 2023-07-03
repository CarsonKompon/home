namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged "electronics".
/// </summary>
[Library( "home_npc_shop_electronics" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Electronics NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopElectronicsNPC : BaseNPC
{

    public override string DisplayName => "#shop.electronics";
    protected override string ClothingString => "[{\"id\":1594058106},{\"id\":1772984322},{\"id\":626961084},{\"id\":-778804355},{\"id\":-620074038},{\"id\":118394888},{\"id\":-1895029643},{\"id\":915965493},{\"id\":-934412006}]";
    
    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), "electronics", "#shop.electronics");

        return false;
    }

}
