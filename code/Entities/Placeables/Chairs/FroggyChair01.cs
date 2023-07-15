using Sandbox;

namespace Home;

/// <summary>
/// A placeable TV that you can queue media on
/// </summary>
[EditorModel("models/froggychair/froggychair.vmdl")]
public partial class FroggyChair01 : ChairBase
{
    public override Transform SeatOffset => new Transform(Vector3.Left * 4 + Vector3.Down * 2 + Vector3.Forward * 4, Rotation.From(0, 90, 0));
    public override Transform ExitOffset => new Transform(Vector3.Up * 20f, Rotation.From(0, 90, 0));
    
    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/froggychair/froggychair.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
    }
}