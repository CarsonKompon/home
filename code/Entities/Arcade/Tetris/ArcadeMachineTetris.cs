using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcade_tetros"), HammerEntity]
[Title("Tetros Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public class ArcadeMachineTetris : ArcadeMachineBase
{
    public override string ControllerType => "ArcadeControllerTetris";

    public override void Spawn()
    {
        base.Spawn();
        
        var test = new TestSprite();
        test.Spawn();
    }
}