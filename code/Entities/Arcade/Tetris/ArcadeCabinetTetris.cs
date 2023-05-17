using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcadecab_tetris"), HammerEntity]
[EditorModel("models/home_dev_arcadecab01.vmdl")]
[Title("Tetris Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public class ArcadeCabinetTetris : Entity
{
    // [Property(Title = "Current Destination" )]
    // public TeleporterDestinations CurrentDestination { get; set; }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        ArcadeCabinetTetrisPanel panel = new ArcadeCabinetTetrisPanel();
        panel.Position = Position + Vector3.Up * 100;
        panel.Rotation = Rotation;
    }
}