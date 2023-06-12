using Sandbox;
using Sandbox.UI;
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
    public WorldPanel Screen { get; set; }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenTetris();
    }

    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        if (Screen == null) return;
        var screenPos = GetAttachment("ScreenPos").Value;
        Screen.Transform = screenPos;
    }
    
}