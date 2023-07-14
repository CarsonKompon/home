using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Editor;
using MediaHelpers;

namespace CarsonK;

/// <summary>
/// A placeable TV that you can queue media on
/// </summary>
[Library("carson_radioplayer"), HammerEntity]
public partial class RadioPlayer : ModelEntity, IUse
{

    public virtual bool IsUsable( Entity user ) => true;
    [Net] public string CurrentlyPlaying { get; set; }
    [Net] public bool IsPlaying { get; set; } = false;

    public MusicPlayer Audio { get; set; }
    public SoundHandle? CurrentSound { get; set; }
    public float Volume { get; set; } = 0.5f;

    RadioNametag Nametag;
    string TitleOverride = "";

    public override void Spawn()
    {
        base.Spawn();

        Model = Cloud.Model("jodiscontent.enhanced_speaker");
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        if(IsPlaying && CurrentlyPlaying != "")
        {
            PlayAudioRpc(To.Single(Client), CurrentlyPlaying);
        }

        Nametag = new RadioNametag(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopAudio();
        Nametag?.Delete();
    }

    public virtual bool OnUse(Entity user)
    {
        Game.AssertServer();

        RadioBrowser.Open(To.Single(user), NetworkIdent);
        
        return false;
    }

    [ConCmd.Server]
    public static void PlayAudio(int id, string url)
    {
        var player = Entity.FindByIndex<RadioPlayer>(id);
        Log.Info(player);
        if(player == null) return;

        Log.Info(url);
        player.CurrentlyPlaying = url;
        player.PlayAudioRpc(url);
    }

    [ClientRpc]
    public async void PlayAudioRpc(string url)
    {
        StopAudio();

        string realUrl = url;
        if(MediaHelper.IsYoutubeUrl(url))
        {
            var player = await MediaHelper.GetYoutubePlayerResponseFromUrl(url);
            realUrl = player.GetStreamUrl();
            TitleOverride = player.Title;
        }
        Log.Info(realUrl);
        Audio = MusicPlayer.PlayUrl(realUrl);
        Audio.Entity = this;
        Audio.Repeat = true;
    }

    public void StopAudio()
    {
        TitleOverride = "";
        if(Audio == null) return;
        Audio.Dispose();
        Audio = null;
    }

    public string GetTitle()
    {
        if(!string.IsNullOrEmpty(TitleOverride)) return TitleOverride;
        if(Audio == null) return "";
        return Audio.Title;
    }

    public void SetVolume(float volume)
    {
        if(Audio == null) return;
        Audio.Volume = volume;
    }
}