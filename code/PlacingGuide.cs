using System;
using Sandbox;

namespace Home;

public static class PlacingGuide
{
    public static bool IsPlacing { get; private set; } = false;
    static string Model { get; set; } = "";
    static Vector3 GhostMins { get; set; } = Vector3.Zero;
    static Vector3 GhostMaxs { get; set; } = Vector3.Zero;

    [GameEvent.Client.Frame]
    public static void OnFrame()
    {
        if ( Game.LocalPawn is not HomePlayer player ) return;
        if(!IsPlacing) return;
        if(!player.CanPlace) return;

        var tr = Trace.Ray(new Ray(Camera.Position, Screen.GetDirection(Mouse.Position)), 1000)
            .Ignore(player.MovingEntity)
            .WithoutTags("player")
            .Run();

        if(tr.Hit)
        {
            player.PlacingPosition = tr.EndPosition;
            var placeable = HomePlaceable.Find(player.Placing);
            var offsetTrans = (Model == "") ? Transform.Zero : placeable.TransformOffset;
            Vector3 surfaceUp = tr.Normal;
            Vector3 surfaceForward = Vector3.Cross(Vector3.Right, surfaceUp).Normal;
            Vector3 surfaceRight = Vector3.Cross(surfaceUp, surfaceForward).Normal;
            Rotation surfaceRotation = Rotation.LookAt(surfaceForward, surfaceUp);
            Rotation spinRotation = Rotation.FromAxis(Vector3.Up, player.PlacingAngle);
            player.PlacingRotation = surfaceRotation * spinRotation;
            if(placeable != null)
            {
                // Rotate the offset by the surface rotation
                Rotation toSurfaceNormal = Rotation.LookAt(surfaceForward, surfaceUp);
                player.PlacingPosition -= toSurfaceNormal * offsetTrans.Position;
                player.PlacingRotation = player.PlacingRotation * offsetTrans.Rotation;
            }
            Gizmo.Draw.Color = Color.White.WithAlpha(0.5f);
            if(Model == "")
            {
                var box = new BBox(GhostMins, GhostMaxs);
                box = box.Transform(new Transform(player.PlacingPosition, player.PlacingRotation));
                Gizmo.Draw.SolidBox(box);
            }
            else
            {
                Gizmo.Draw.Model(Model, new Transform(player.PlacingPosition, player.PlacingRotation));
            }
        }
    }

    public static async void StartPlacing(HomePlaceable placeable)
    {
        IsPlacing = true;
        Model = "";
        var _cloudIdent = placeable.CloudIdent;
        if(placeable.CloudModel != "") _cloudIdent = placeable.CloudModel;
        if(!string.IsNullOrEmpty(_cloudIdent))
        {
            var package = await Package.FetchAsync(_cloudIdent, true);
            if(package != null)
            {
                GhostMins = package.GetMeta("RenderMins", Vector3.Zero);
                GhostMaxs = package.GetMeta("RenderMaxs", Vector3.Zero);
                await package.MountAsync();
                placeable.Model = package.GetMeta("PrimaryAsset", "");
                if(string.IsNullOrEmpty(placeable.Model)) Model = "models/dev/error.vmdl";
                else Model = placeable.Model;
                return;
            }
        }

        Model = placeable.Model;
    }

    public static void StopPlacing()
    {
        Model = "";
        IsPlacing = false;
    }
}