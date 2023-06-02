using System;
using Sandbox;

namespace Home;

public class PlacingGuide : Entity
{
    [GameEvent.Client.Frame]
    public static void OnFrame()
    {
        if ( Game.LocalPawn is not HomePlayer player ) return;
        if(player.PlacingModel == "") return;
        if(!player.CanPlace) return;

        var tr = Trace.Ray(new Ray(Camera.Position, Screen.GetDirection(Mouse.Position)), 1000)
            .Ignore(player.MovingEntity)
            .WithoutTags("player")
            .Run();

        if(tr.Hit)
        {
            player.PlacingPosition = tr.EndPosition;
            Vector3 surfaceUp = tr.Normal;
            Vector3 surfaceForward = Vector3.Cross(Vector3.Right, surfaceUp).Normal;
            Vector3 surfaceRight = Vector3.Cross(surfaceUp, surfaceForward).Normal;
            Rotation surfaceRotation = Rotation.LookAt(surfaceForward, surfaceUp);
            Rotation spinRotation = Rotation.FromAxis(Vector3.Up, player.PlacingAngle);
            player.PlacingRotation = surfaceRotation * spinRotation;
            Gizmo.Draw.Color = Color.White.WithAlpha(0.5f);
            Gizmo.Draw.Model(player.PlacingModel, new Transform(player.PlacingPosition, player.PlacingRotation));
        }
    }
}