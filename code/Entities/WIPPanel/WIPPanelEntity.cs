using Sandbox;
using Sandbox.UI;
using Editor;

namespace Home;

[Library("home_wip_screen"), HammerEntity]
[Title("WIP Screen"), Category("Other"), Icon("aod")]
public class WIPPanelEntity : Entity
{
    public WorldPanel Screen { get; set; }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        Screen = new WIPPanel();
        Screen.Transform = Transform;
        Screen.Position += Vector3.Up * 60f;
    }

}