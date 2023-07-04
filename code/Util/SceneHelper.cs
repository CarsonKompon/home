using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace Home.Util;

public static class SceneHelper
{

    private static Dictionary<string, Texture> ModelThumbnails = new Dictionary<string, Texture>();
    public static Texture CreateModelThumbnail(string modelString, int size = 96)
    {
        if(ModelThumbnails.ContainsKey(modelString))
        {
            return ModelThumbnails[modelString];
        }

        // Thanks xenthio for this math :)
        float scalar = 3f;
        Model model = Model.Load(modelString);
        Texture texture = Texture.CreateRenderTarget()
                        .WithHeight(size).WithWidth(size)
                        .Create();
        var scene = new SceneCamera();
        scene.AntiAliasing = true;
        scene.World = new SceneWorld();
        scene.FieldOfView = 10;
        var maxBound = model.Bounds.Size.Length;
        var dist = (maxBound / scalar) / MathF.Tan(MathX.DegreeToRadian(scene.FieldOfView / 2));
        
        SceneObject obj = new SceneObject(scene.World, model, Transform.Zero);
        var box = (obj.Bounds.Mins + obj.Bounds.Maxs) / 2;
        scene.Position = new Vector3(1, 1, 0.9f) * dist;
        scene.Rotation = Rotation.LookAt(-1 * (scene.Position - box).Normal);
        var light = new SceneLight(scene.World, new Vector3(32, 128, 128), 1024, Color.White);
        light.LightColor = Color.White.Lighten(2);
        light.ShadowsEnabled = true;
        scene.AmbientLightColor = Color.White.Darken(0.5f);

        Graphics.RenderToTexture(scene, texture);
        ModelThumbnails.Add(modelString, texture);

        obj.Delete();
        scene.World.Delete();
        return texture;
    }

    private static Dictionary<string, Texture> ClothingThumbnails = new Dictionary<string, Texture>();
    public static Texture CreateClothingThumbnail(Clothing resource, int size = 256)
    {
        if(ClothingThumbnails.ContainsKey(resource.ResourcePath))
        {
            return ClothingThumbnails[resource.ResourcePath];
        }

		var Scene = new ClothingScene();
		Scene.UpdateLighting();
		Scene.InstallClothing( resource );
		Scene.UpdateCameraPosition();

        Texture texture = Texture.CreateRenderTarget()
            .WithHeight(size).WithWidth(size)
            .Create();

        Graphics.RenderToTexture(Scene.Camera, texture);
        ClothingThumbnails.Add(resource.ResourcePath, texture);

        Scene.World.Delete();
		return texture;
    }
}