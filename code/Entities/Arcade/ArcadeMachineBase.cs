using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[EditorModel("models/arcade/arcade_machine_dev_01.vmdl")]
public partial class ArcadeMachineBase : ModelEntity, IUse
{
    public virtual bool IsUsable( Entity user ) => true;
    [Net] public HomePlayer CurrentUser { get; set; } = null;
    public bool InUse => CurrentUser != null;

    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/arcade/arcade_machine_dev_01.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Static);
    }

    public virtual void StartGame()
    {

    }

    public virtual void EndGame()
    {

    }

    public void SetUser(HomePlayer player)
    {
        if(player.Controller is ArcadeControllerBase controller)
        {
            NotificationPanel.AddEntry(To.Single(player), "🚫 You are already using an arcade machine.", "", 3);
            return;
        }
        CurrentUser = player;
        var arcadeController = new ArcadeControllerBase();
        arcadeController.ArcadeMachine = this;
        player.Controller = arcadeController;
        StartGame();
    }

    public void RemoveUser()
    {
        if(CurrentUser == null) return;
        
        EndGame();

        if(CurrentUser.Controller is ArcadeControllerBase)
        {
            CurrentUser.Controller = new HomeWalkController();
        }
        CurrentUser = null;
    }

    public virtual bool OnUse(Entity user)
    {
        Game.AssertServer();
        if(user is not HomePlayer player) return false;
        if(CurrentUser == null)
        {
            SetUser(player);
        }
        else
        {
            NotificationPanel.AddEntry(To.Single(user), "🚫 This machine is currently in use.", "", 3);
        }
        return false;
    }
}