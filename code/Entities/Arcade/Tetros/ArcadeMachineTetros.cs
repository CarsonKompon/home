using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;
using Tetros;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcade_tetros"), HammerEntity]
[Title("Tetros Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public partial class ArcadeMachineTetros : ArcadeMachineBase
{
    public ArcadeScreenTetros Screen { get; set; }

    public override string ControllerType => "ArcadeControllerTetros";

    public override void Spawn()
    {
        base.Spawn();
        SetMaterialOverride(Cloud.Material("shadb.tetros_cabinet"));
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenTetros(this);
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

    public override void EndGame(long steamId)
    {
        EndGameRpc(steamId);

        base.EndGame(steamId);
    }

    [ClientRpc]
    public void EndGameRpc(long steamId)
    {
        Screen?.Menu?.EndGame.Invoke(steamId);
    }

    [ConCmd.Server]
    public static void RequestUpdateBoard(int ident, string board)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetros;
        if(machine == null) return;
        if(machine.CurrentUser == null) return;
        machine.UpdateBoardRpc(board);
    }

    [ClientRpc]
    public void UpdateBoardRpc(string board)
    {
        if(Game.LocalClient == CurrentUser) return;
        Screen?.Menu?.UpdateBoard.Invoke(TetrosGamePage.StringToBoard(board));
    }

    [ConCmd.Server]
    public static void RequestUpdatePlayer(int ident, int blockType, int x, int y, int rot)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetros;
        if(machine == null) return;
        if(machine.CurrentUser == null) return;
        machine.UpdatePlayerRpc(blockType, x, y, rot);
    }

    [ClientRpc]
    public void UpdatePlayerRpc(int blockType, int x, int y, int rot)
    {
        if(Game.LocalClient == CurrentUser) return;
        Screen?.Menu?.UpdatePlayer.Invoke((BlockType)blockType, new Vector2(x, y), rot);
    }

    [ConCmd.Server]
    public static void RequestHeldPiece(int ident, int blockType)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetros;
        if(machine == null) return;
        machine.UpdateHeldPieceRpc(blockType);
    }

    [ClientRpc]
    public void UpdateHeldPieceRpc(int blockType)
    {
        if(Game.LocalClient == CurrentUser) return;
        Screen?.Menu?.UpdateHeldPiece.Invoke((BlockType)blockType);
    }

    [ConCmd.Server]
    public static void RequestNextPieces(int ident, string queueStr)
    {
        var machine = Entity.FindByIndex<ArcadeMachineTetros>(ident);
        if(machine == null) return;
        var queue = TetrosGamePage.StringToBoard(queueStr);
        machine.UpdateNextPiecesRpc(queue);
    }

    [ClientRpc]
    public void UpdateNextPiecesRpc(int[] queue)
    {
        if(Game.LocalPawn == CurrentUser) return;
        Screen?.Menu?.UpdateNextPieces.Invoke(queue.Select(i => (BlockType)i).ToArray());
    }

    [ConCmd.Server]
    public static void RequestScore(int ident, long score)
    {
        var machine = Entity.FindByIndex(ident) as ArcadeMachineTetros;
        if(machine == null) return;
        machine.UpdateScoreRpc(score);
    }

    [ClientRpc]
    public void UpdateScoreRpc(long score)
    {
        if(Game.LocalClient == CurrentUser) return;
        Screen?.Menu?.UpdateScore.Invoke(score);
    }

    // [ConCmd.Server]
    // public static void RequestHideStuff(int ident)
    // {
    //     var machine = Entity.FindByIndex(ident) as ArcadeMachineTetros;
    //     if(machine == null) return;
    //     machine.HideAllRpc();
    // }

    // [ClientRpc]
    // public void HideAllRpc()
    // {
    //     Screen?.HideAll();
    // }

    // [ConCmd.Server]
    // public static void RequestShowStuff(int ident)
    // {
    //     var machine = Entity.FindByIndex(ident) as ArcadeMachineTetros;
    //     if(machine == null) return;
    //     machine.ShowAllRpc();
    // }

    // [ClientRpc]
    // public void ShowAllRpc()
    // {
    //     Screen?.ShowAll();
    // }

}