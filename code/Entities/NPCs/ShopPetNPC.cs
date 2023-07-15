using Sandbox;
using Editor;
using Home.Util;

namespace Home;

/// <summary>
/// Talking to this NPC will test a crash.
/// </summary>
[Library( "home_npc_shop_pet" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Pet NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopPetNPC : BaseNPC
{

    public override string DisplayName => "#shop.pets";
    protected override string ClothingString => "[{\"id\":-1191994301},{\"id\":1772984322},{\"id\":1594058106},{\"id\":5927228},{\"id\":-691668871},{\"id\":469696431},{\"id\":667697466},{\"id\":1332484731}]";

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPets.Open(To.Single(user), "#shop.pets");

        return false;
    }

}