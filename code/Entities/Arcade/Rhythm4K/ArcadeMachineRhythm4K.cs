using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;
using Rhythm4K;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcade_rhythm"), HammerEntity]
[Title("rhythm4K Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
[EditorModel("models/carsonhome/ddrmachine.vmdl")]
public partial class ArcadeMachineRhythm4K : ArcadeMachineBase
{
    public ArcadeScreenRhythm4K Screen { get; set; }

    public override void Spawn()
    {
        base.Spawn();

        SetModel("models/carsonhome/ddrmachine.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Static);

        if(IsFromMap)
        {
            Random rand = new Random();
            RenderColor = new ColorHsv(rand.Next(0, 360), 0.85f, 1).ToColor();
        }
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenRhythm4K(this);
    }

    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        if (Screen == null) return;
        var screenPos = GetAttachment("ScreenPos").Value;
        Screen.Transform = screenPos;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(Game.IsClient) Screen?.Delete();
    }

    public override void StartGame()
    {
        base.StartGame();
        
        StartGameRpc(To.Single(CurrentUser));

        // UpdateBoard();
        // UpdateNextPieces();
        // UpdateHeldPiece();
    }

    [ClientRpc]
    public void StartGameRpc()
    {
        Screen?.StartGame();
    }

    [ConCmd.Server]
    public static void Payout(long steamId, long score)
    {
        var user = Game.Clients.FirstOrDefault(c => c.SteamId == steamId);
        if(user == null) return;
        if(user.Pawn is not HomePlayer player) return;
        player.GiveMoney(score);
    }

    public override void EndGame()
    {
        base.EndGame();

        EndGameRpc(To.Single(CurrentUser));
    }

    [ClientRpc]
    public void EndGameRpc()
    {
        if(Screen.Descendants.FirstOrDefault(x => x is GamePage) is GamePage gp)
        {
            gp.EndSong();
            gp.Navigate("/");
        }
        else if(Screen.Descendants.FirstOrDefault(x => x is Rhythm4KSingleMenu) is Rhythm4KSingleMenu menu)
        {
            menu.Navigate("/");
        }
    }

}