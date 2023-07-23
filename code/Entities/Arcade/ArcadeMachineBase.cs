using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[EditorModel("models/arcade/cabinet/cabinet.vmdl")]
public partial class ArcadeMachineBase : AnimatedEntity, IUse
{
    public virtual bool IsUsable( Entity user ) => true;
    [Net] public HomePlayer CurrentUser { get; set; } = null;
    [Net] public long PreviousUserSteamId { get; set; } = 0;
    public bool InUse => CurrentUser != null;

    public virtual string ControllerType => "ArcadeControllerBase";

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

    [ConCmd.Server]
    public static void Payout(long steamId, long score)
    {
        var user = Game.Clients.FirstOrDefault(c => c.SteamId == steamId);
        if(user == null) return;
        if(user.Pawn is not HomePlayer player) return;
        player.GiveMoney(score);
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

        var type = TypeLibrary.GetType<ArcadeControllerBase>(ControllerType)?.TargetType;
        if(type == null) return;
        var arcadeController = TypeLibrary.Create<ArcadeControllerBase>(type);
        arcadeController.ArcadeMachine = this;
        Log.Info(arcadeController.ArcadeMachine);

        player.SetController(arcadeController);
        PreviousUserSteamId = player.Client.SteamId;
        StartGame();
    }

    public void RemoveUser()
    {
        Game.AssertServer();
        if(CurrentUser == null) return;

        if(CurrentUser?.Controller is ArcadeControllerBase controller)
        {
            controller.OnExit();
        }

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

    // [GameEvent.Tick.Client]
    // public void OnClientTick()
    // {
    //     Gizmo.Draw.SolidSphere(GetAttachment("hand_L")?.Position ?? Vector3.Zero, 1);
    //     Gizmo.Draw.SolidSphere(GetAttachment("hand_R")?.Position ?? Vector3.Zero, 1);
    // }
}