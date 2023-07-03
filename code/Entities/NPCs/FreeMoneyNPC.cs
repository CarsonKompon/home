namespace Home;

/// <summary>
/// Talking to this NPC will give the player free money.
/// </summary>
[Library( "home_npc_free_money" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Free Money NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class FreeMoneyNPC : BaseNPC
{

    public override string DisplayName => "#npc.freemoney";
    protected override string ClothingString => "[{\"id\":-1940305134},{\"id\":-620074038},{\"id\":1772984322},{\"id\":-1630059189},{\"id\":-253050224},{\"id\":2140927486},{\"id\":-1611545262}]";

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
