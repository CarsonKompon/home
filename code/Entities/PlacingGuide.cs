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

        var tr = Trace.Ray(new Ray(Camera.Position, Screen.GetDirection(Mouse.Position)), 1000).Ignore(Game.LocalPawn).Run();
        if(tr.Hit)
        {
            player.PlacingPosition = tr.EndPosition;
            Gizmo.Draw.Color = Color.White.WithAlpha(0.65f);
            Gizmo.Draw.Model(player.PlacingModel, new Transform(player.PlacingPosition, Rotation.From(0, player.PlacingRotation, 0)));
        }
    }
}