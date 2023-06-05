using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will allow players to check-in to an available room.
/// </summary>
[Library( "home_npc_shop_foliage" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Shop Foliage NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class ShopFoliageNPC : BaseNPC
{

    public ShopFoliageNPC()
    {
        DisplayName = "Foliage Shop";
        ClothingString = "[{\"id\":1594058106},{\"id\":1772984322},{\"id\":-691668871},{\"id\":469696431},{\"id\":358536239},{\"id\":-2122754807},{\"id\":720795379},{\"id\":-1346233164},{\"id\":197300820},{\"id\":533938816}]";
    }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        UseAnimGraph = true;
        SetAnimParameter( "sit", 2 );
        SetAnimParameter( "sit_pose", 3);
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopFoliage.Open(To.Single(user));

        return false;
    }

}