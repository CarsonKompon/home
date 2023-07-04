using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Home;

public class ClothingScene
{
    public SceneWorld World;
    public SceneCamera Camera;

    public SceneModel Body;

    public SceneModel TargetModel;

    List<SceneLight> Lights = new();
    
    public SceneSunLight Sun;

    public float Pitch = 15.0f;
    public float Yaw = 35.0f;
    public SlotMode Target = SlotMode.ModelBounds;

    public enum SlotMode
    {
        ModelBounds,
        Face
    }

    public ClothingScene()
    {
        World = new SceneWorld();
        Camera = new SceneCamera( "ClothingEditor" );

        Body = new SceneModel( World, "models/citizen/citizen.vmdl", Transform.Zero );
        Body.Rotation = Rotation.From( 0, 0, 0 );
        Body.Position = 0;
        Body.SetAnimParameter( "b_grounded", true );
        Body.SetAnimParameter( "aim_eyes", Vector3.Forward * 100.0f );
        Body.SetAnimParameter( "aim_head", Vector3.Forward * 100.0f );
        Body.SetAnimParameter( "aim_body", Vector3.Forward * 100.0f );
        Body.SetAnimParameter( "aim_body_weight", 1.0f );
        Body.Update( 1 );

        Camera.World = World;
        Camera.BackgroundColor = new Color( 0.1f, 0.1f, 0.1f, 0.0f );
        Camera.AmbientLightColor = Color.Gray * 0.1f;
        
        TargetModel = Body;
    }

    public void InstallClothing( Clothing clothing )
    {
        var created = Clothing.DressSceneObject( Body, new Clothing[] { clothing } );
        TargetModel = created.FirstOrDefault();

        if ( TargetModel == null )
        {
            TargetModel = Body;
            Target = SlotMode.Face;
            return;
        }

        if ( clothing.SlotsUnder.HasFlag( Clothing.Slots.EyeBrows ) ||
            clothing.SlotsUnder.HasFlag( Clothing.Slots.Face ) ||
            clothing.SlotsUnder.HasFlag( Clothing.Slots.Glasses ) ||
            clothing.SlotsUnder.HasFlag( Clothing.Slots.HeadTop ) )
        {
            Target = SlotMode.Face;
        }

        var greySkin = Material.Load( "models/citizen/skin/citizen_skin_grey.vmat" );

        Body.SetMaterialOverride( greySkin, "skin" );
        //Body.SetMaterialOverride( greySkin, "eyes" );
        //Body.SetMaterialOverride( greySkin, "eyeao" );

        TargetModel.SetMaterialOverride( greySkin, "skin" );
        //TargetModel.SetMaterialOverride( greySkin, "eyes" );

        TargetModel.Update( 1 );
    }

    public void UpdateLighting()
    {
        foreach( var light in Lights )
        {
            light.Delete();
        }
        Lights.Clear();

        Sun = new SceneSunLight( World, Rotation.From( 45, -180, 0 ), Color.White * 0.5f + Color.Cyan * 0.05f );
        Sun.ShadowsEnabled = true;
        Sun.SkyColor = Color.White * 0.05f + Color.Cyan * 0.025f;

        new SceneCubemap( World, Texture.Load( "textures/cubemaps/default.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) );

        Lights.Add( new SceneLight( World, new Vector3( -100, -10, 60 ), 200, new Color( 0.8f, 1, 1 ) * 1.3f ) { ShadowTextureResolution = 512 } );
        Lights.Add( new SceneLight( World, new Vector3( -100, 150, 60 ), 400, new Color( 1, 0.9f, 0.6f ) * 16.0f ) { ShadowTextureResolution = 512 } );
        Lights.Add( new SceneLight( World, new Vector3( 200, 50, 500 ), 1200, new Color( 1, 0.9f, 0.85f ) * 20.0f ) { ShadowTextureResolution = 512 } );
    }

    public void UpdateCameraPosition()
    {
        if ( TargetModel == null )
            return;

        Camera.FieldOfView = 5;
        Camera.ZFar = 2000;
        Camera.ZNear = 10;

        var bounds = TargetModel.Bounds;

        if ( Target == SlotMode.ModelBounds )
        {
            bounds = TargetModel.Bounds;
        }
        else if ( Target == SlotMode.Face )
        {
            var headBone = TargetModel.GetBoneWorldTransform( "head" );
            headBone.Position += Vector3.Up * 6;
            bounds = new BBox( headBone.Position - 7, headBone.Position + 7 );
        }

        var lookAngle = new Angles( Pitch, 180 - Yaw, 0 );
        var forward = lookAngle.Forward;
        var distance = MathX.SphereCameraDistance( bounds.Size.Length * 0.5f, Camera.FieldOfView );

        Camera.Position = bounds.Center - forward * distance;
        Camera.Rotation = Rotation.From( lookAngle );
    }
}