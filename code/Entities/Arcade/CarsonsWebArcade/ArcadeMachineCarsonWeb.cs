using System.Reflection.PortableExecutable;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;
using CarsonsWebArcade;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcade_carson_web"), HammerEntity]
[Title("Carson's Web Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public partial class ArcadeMachineCarsonWeb : ArcadeMachineBase
{
    public ArcadeScreenCarsonWeb Screen { get; set; }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenCarsonWeb(this);
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
    }

    [ClientRpc]
    public void StartGameRpc()
    {
        Screen?.StartGame();
    }

    public override void EndGame(long steamId)
    {
        EndGameRpc(To.Single(CurrentUser), steamId);

        base.EndGame(steamId);
    }

    [ClientRpc]
    public void EndGameRpc(long steamId)
    {
        Screen?.EndGame();
    }

}