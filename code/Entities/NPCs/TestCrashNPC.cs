using Sandbox;
using Editor;
using Home.Util;

namespace Home;

/// <summary>
/// Talking to this NPC will test a crash.
/// </summary>
[Library( "home_npc_test_crash" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Test Crash NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class TestCrashNPC : BaseNPC
{

    public override string DisplayName => "Fix MediaPlayers";
    protected override string ClothingString => "[{\"id\":-293856662}]";

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(Game.IsServer)
        {
            TestCrash(To.Single(user));
        }

        return false;
    }

    [ClientRpc]
    public async void TestCrash()
    {
        NotificationPanel.AddEntry("The issue has been fixed by facepunch :)", "", 5);
    }

}