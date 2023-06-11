// using Sandbox;
// using Sandbox.UI;
// using Editor;
// using System.Linq;

// namespace Home;

// /// <summary>
// /// This is a teleporter that allows you to teleport around the lobby
// /// </summary>
// [EditorModel("models/arcade/arcade_machine_dev_01.vmdl")]
// public partial class ArcadeMachineBase : ModelEntity, IUse
// {
//     public virtual string ControllerType => "ArcadeControllerBase";

//     public virtual bool IsUsable( Entity user ) => true;
//     [Net] public HomePlayer CurrentUser { get; set; } = null;
//     public bool InUse => CurrentUser != null;
//     public Vector2 ScreenSize { get; set; } = new Vector2(32, 24);
//     public Texture ScreenTexture { get; set; }
//     public Material ScreenMaterial { get; set; }
    
//     public ArcadeMachineBase()
//     {
//         ScreenTexture = Texture.CreateRenderTarget()
//                         .WithSize(ScreenSize)
//                         .WithFormat(ImageFormat.RGBA32323232F)
//                         .WithScreenMultiSample()
//                         .Create();

//         ScreenMaterial = Material.Load("materials/arcade/arcade_screen.vmat");
//     }

//     public override void Spawn()
//     {
//         base.Spawn();
//         SetModel("models/arcade/arcade_machine_dev_01.vmdl");
//         SetupPhysicsFromModel(PhysicsMotionType.Static);
//     }

//     public override void ClientSpawn()
//     {
//         base.ClientSpawn();
//     }

//     public virtual void StartGame()
//     {

//     }

//     public virtual void EndGame()
//     {

//     }

//     public void SetUser(HomePlayer player)
//     {
//         if(player.Controller is ArcadeControllerBase controller)
//         {
//             NotificationPanel.AddEntry(To.Single(player), "🚫 You are already using an arcade machine.", "", 3);
//             return;
//         }
//         CurrentUser = player;
//         var type = TypeLibrary.GetType<ArcadeControllerBase>(ControllerType)?.TargetType;
//         if(type == null) return;
//         var arcadeController = TypeLibrary.Create<ArcadeControllerBase>(type);
//         arcadeController.ArcadeMachine = this;
//         player.Controller = arcadeController;
//         StartGame();
//     }

//     public void RemoveUser()
//     {
//         if(CurrentUser == null) return;
        
//         if(CurrentUser.Controller is ArcadeControllerBase)
//         {
//             CurrentUser.Controller = new HomeWalkController();
//         }
//         CurrentUser = null;
//         EndGame();
//     }

//     public virtual bool OnUse(Entity user)
//     {
//         Game.AssertServer();
//         if(user is not HomePlayer player) return false;
//         if(CurrentUser == null)
//         {
//             SetUser(player);
//         }
//         else
//         {
//             NotificationPanel.AddEntry(To.Single(user), "🚫 This machine is currently in use.", "", 3);
//         }
//         return false;
//     }

//     [GameEvent.Tick.Client]
//     public void OnFrame()
//     {
//         if(Camera.Main == null) return;
//         Graphics.RenderToTexture(Camera.Main, ScreenTexture);
//         ScreenMaterial.Set("color", ScreenTexture);
//         SetMaterialOverride(ScreenMaterial, "screen");
//         Log.Info("h");
//     }
// }


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
    public virtual string ControllerType => "ArcadeControllerBase";

    public virtual bool IsUsable( Entity user ) => true;
    [Net] public HomePlayer CurrentUser { get; set; } = null;
    public bool InUse => CurrentUser != null;
    public SceneWorld World { get; set; }
    public SceneCamera Camera { get; set; }
    public Texture ScreenTexture { get; set; }
    public Material ScreenMaterial { get; set; }

    public ArcadeRenderHook RenderHook { get; set; }
    public Vector2 ScreenSize { get; set; } = new Vector2(320, 240);

    public ArcadeMachineBase()
    {
        ScreenTexture = Texture.CreateRenderTarget()
                        .WithSize(ScreenSize)
                        .WithScreenFormat()
                        .WithScreenMultiSample()
                        .Create();

        ScreenMaterial = Material.Load("materials/arcade/arcade_screen.vmat");
    
        if(Game.IsClient)
        {
            World = new SceneWorld();

            Camera = new SceneCamera("ArcadeCamera " + Name);
            Camera.World = World;
            Camera.Ortho = true;
            Camera.OrthoHeight = ScreenSize.y;
            Camera.OrthoWidth = ScreenSize.y * (32.44f / 26.77f);
            Camera.ZNear = 1f;
            Camera.ZFar = 1024f;
            Camera.Position = new Vector3(0, 0, 512f);
            Camera.Rotation = Rotation.FromYaw(180f);
            Camera.AmbientLightColor = Color.White;
            Camera.BackgroundColor = Color.Green;
        }
    }

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
        var type = TypeLibrary.GetType<ArcadeControllerBase>(ControllerType)?.TargetType;
        if(type == null) return;
        var arcadeController = TypeLibrary.Create<ArcadeControllerBase>(type);
        arcadeController.ArcadeMachine = this;
        player.Controller = arcadeController;
        StartGame();
    }

    public void RemoveUser()
    {
        if(CurrentUser == null) return;
        
        if(CurrentUser.Controller is ArcadeControllerBase)
        {
            CurrentUser.Controller = new HomeWalkController();
        }
        CurrentUser = null;
        EndGame();
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
    // public void OnFrame()
    // {
    //     if(ScreenTexture == null) return;
    //     Graphics.RenderToTexture(ScenePanel.Camera, ScreenTexture);
    //     Material mat = Material.Load("materials/arcade/arcade_screen.vmat");
    //     Log.Info(ScreenTexture.GetPixels()[92]);
    //     mat.Set("color", ScreenTexture);
    //     SetMaterialOverride(mat, "screen");
    // }

    [GameEvent.PreRender]
    public void OnFrame()
    {
        Graphics.RenderToTexture(Camera, ScreenTexture);
        ScreenMaterial.Set("color", ScreenTexture);
        SetMaterialOverride(ScreenMaterial, "screen");
    }
}

public partial class ArcadeRenderHook : RenderHook
{
    public override void OnStage(SceneCamera target, Stage renderStage)
    {
        if(renderStage == Stage.AfterUI)
        {
            target.BackgroundColor = Color.Green;
        
            
        }
    }
}