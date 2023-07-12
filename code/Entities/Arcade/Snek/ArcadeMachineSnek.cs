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
[Library("home_arcade_snek"), HammerEntity]
[Title("Snek Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public partial class ArcadeMachineSnek : ArcadeMachineBase
{
    public ArcadeScreenSnek Screen { get; set; }

    public override void Spawn()
    {
        base.Spawn();
        SetMaterialOverride(Cloud.Material("shadb.snek_cabinet"));
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenSnek(this);
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
    }

    [ClientRpc]
    public void StartGameRpc()
    {
        Screen?.StartGame();
    }

    // public override void EndGame(long steamId)
    // {
    //     EndGameRpc(steamId);

    //     base.EndGame(steamId);
    // }

    // [ClientRpc]
    // public void EndGameRpc(long steamId)
    // {
    //     Screen?.Menu?.EndGame.Invoke(steamId);
    // }

}