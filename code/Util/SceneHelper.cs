namespace Home.Util;

public static class SceneHelper
{
    public static Texture CreateModelThumbnail(string modelString, int size = 96)
    {
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
        obj.Delete();
        scene.World.Delete();
        return texture;
    }
}
