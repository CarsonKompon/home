using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// A placeable TV that you can queue media on
/// </summary>
[EditorModel("models/sbox_props/office_chair/office_chair.vmdl")]
public partial class OfficeChair01 : ChairBase
{
    public override Transform SeatOffset => new Transform(Vector3.Left * 8 + Vector3.Down * 4, Rotation.From(0, 90, 0));
    public override Transform ExitOffset => new Transform(Vector3.Up * 20f, Rotation.From(0, 90, 0));
    
    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/sbox_props/office_chair/office_chair.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
    }
}