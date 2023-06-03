using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will allow players to check-in to an available room.
/// </summary>
[Library( "home_npc_free_money" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Free Money NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class FreeMoneyNPC : BaseNPC
{

    public FreeMoneyNPC()
    {
        DisplayName = "Free Money";
        ClothingString = "[{\"id\":-1940305134},{\"id\":-620074038},{\"id\":1772984322},{\"id\":-1630059189},{\"id\":-253050224},{\"id\":2140927486},{\"id\":-1611545262}]";
        //[{"id":-1266562526},{"id":-1901789076},{"id":56388154},{"id":1977425295},{"id":791197114},{"id":626961084},{"id":1254089650},{"id":915965493},{"id":591027714}]
    }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        if (user is HomePlayer player)
        {
            player.GiveMoney(1000);
        }

        return false;
    }

}