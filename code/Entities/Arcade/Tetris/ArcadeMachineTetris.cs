using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcade_tetros"), HammerEntity]
[Title("Tetros Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public partial class ArcadeMachineTetris : ArcadeMachineBase
{
    public override string ControllerType => "ArcadeControllerTetris";
    public ArcadeScreenTetris Screen { get; set; }

    public override void Spawn()
    {
        // TODO: Remove this hack once map uploading is fixed
        if(IsFromMap)
        {
            Rotation = Rotation.FromYaw(180);
        }

        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenTetris(this);
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
        
        StartGameRpc();

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

    [ConCmd.Server]
    public static void RequestEndGame(int ident)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        if(machine.CurrentUser == null) return;
        machine.RemoveUser();
    }

    public override void EndGame()
    {
        EndGameRpc(CurrentUser.Client.SteamId);

        base.EndGame();
    }

    [ClientRpc]
    public void EndGameRpc(long steamId)
    {
        Screen?.EndGame(steamId);
    }

    [ConCmd.Server]
    public static void RequestUpdateBoard(int ident, string board)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        if(machine.CurrentUser == null) return;
        var clients = Game.Clients.Where(c => c.SteamId != machine.CurrentUser.Client.SteamId);
        machine.UpdateBoardRpc(To.Multiple(clients), board);
    }

    [ClientRpc]
    public void UpdateBoardRpc(string board)
    {
        Screen?.UpdateBoard(ArcadeScreenTetris.StringToBoard(board));
    }

    [ConCmd.Server]
    public static void RequestUpdatePlayer(int ident, int blockType, int x, int y, int rot)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        var clients = Game.Clients.Where(c => c.SteamId != machine.CurrentUser.Client.SteamId);
        machine.UpdatePlayerRpc(To.Multiple(clients), blockType, x, y, rot);
    }

    [ClientRpc]
    public void UpdatePlayerRpc(int blockType, int x, int y, int rot)
    {
        Screen?.UpdatePlayer((ArcadeScreenTetris.BlockType)blockType, new Vector2(x, y), rot);
    }

    [ConCmd.Server]
    public static void RequestHeldPiece(int ident, int blockType)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        var clients = Game.Clients.Where(c => c.SteamId != machine.CurrentUser.Client.SteamId);
        machine.UpdateHeldPieceRpc(To.Multiple(clients), blockType);
    }

    [ClientRpc]
    public void UpdateHeldPieceRpc(int blockType)
    {
        Screen?.UpdateHeldPiece((ArcadeScreenTetris.BlockType)blockType);
    }

    [ConCmd.Server]
    public static void RequestNextPieces(int ident, string queueStr)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        var queue = ArcadeScreenTetris.StringToBoard(queueStr);
        var clients = Game.Clients.Where(c => c.SteamId != machine.CurrentUser.Client.SteamId);
        machine.UpdateNextPiecesRpc(queue);
    }

    [ClientRpc]
    public void UpdateNextPiecesRpc(int[] queue)
    {
        Screen?.UpdateNextPieces(queue.Select(i => (ArcadeScreenTetris.BlockType)i).ToArray());
    }

    [ConCmd.Server]
    public static void RequestScore(int ident, long score)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        machine.UpdateScoreRpc(score);
    }

    [ClientRpc]
    public void UpdateScoreRpc(long score)
    {
        Screen?.UpdateScore(score);
    }

    [ConCmd.Server]
    public static void RequestHideStuff(int ident)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        machine.HideAllRpc();
    }

    [ClientRpc]
    public void HideAllRpc()
    {
        Screen?.HideAll();
    }

    [ConCmd.Server]
    public static void RequestShowStuff(int ident)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetris;
        if(machine == null) return;
        machine.ShowAllRpc();
    }

    [ClientRpc]
    public void ShowAllRpc()
    {
        Screen?.ShowAll();
    }

}