using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[EditorModel("models/arcade/cabinet/cabinet.vmdl")]
public partial class ArcadeMachineBase : ModelEntity, IUse
{
    public virtual bool IsUsable( Entity user ) => true;
    [Net] public HomePlayer CurrentUser { get; set; } = null;
    public bool InUse => CurrentUser != null;

    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/arcade/cabinet/cabinet.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Static);
    }

    public virtual void StartGame()
    {

    }

    public virtual void EndGame(long steamId)
    {
        RequestRemoveUser(NetworkIdent);
    }

    [ConCmd.Server]
    public static void RequestRemoveUser(int networkIdent)
    {
        var machine = Entity.FindByIndex<ArcadeMachineBase>(networkIdent);
        if(machine == null) return;
        machine.RemoveUser();
    }

    public void SetUser(HomePlayer player)
    {
        Game.AssertServer();
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
        Game.AssertServer();
        Log.Info("trying to remove...");
        if(CurrentUser == null) return;

        CurrentUser.ResetController();
        CurrentUser = null;
        Log.Info("removed");
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