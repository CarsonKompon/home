using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;
using Home.Utils;

namespace Home;

/// <summary>
/// A placeable TV that you can queue media on
/// </summary>
[EditorModel("models/entities/tv_floor_01.vmdl")]
public partial class PlaceableTV : ModelEntity, IUse
{

    public virtual bool IsUsable( Entity user ) => true;
    public List<string> Queue = new();

    public VideoPlayer Video { get; set; }
    public Material ScreenMaterial { get; set; }

    public PlaceableTV()
    {
        if(Game.IsClient)
        {
            Video = new VideoPlayer();
            Video.OnAudioReady = () => Video.PlayAudio(this);
        }

        ScreenMaterial = Material.Load("materials/arcade/arcade_screen.vmat").CreateCopy();
    }

    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/entities/tv_floor_01.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Video?.Dispose();
    }

    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        Video?.Present();
        ScreenMaterial.Set("Color", Video.Texture);
        SetMaterialOverride(ScreenMaterial, "screen");
    }

    public virtual bool OnUse(Entity user)
    {
        Game.AssertServer();

        PlayYoutubeVideo("dQw4w9WgXcQ");
        
        return false;
    }

    [ClientRpc]
    public async void PlayYoutubeVideo(string url)
    {
        var whoa = await MediaHelper.GetUrlFromYoutubeId(url);
        Video.Play(whoa);
    }
}