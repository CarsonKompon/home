using System;
using Sandbox;

namespace Home;

public class RoomPropStatic : Prop
{

    public RoomPropStatic()
    {
    }

    public RoomPropStatic(string model)
    {
        SetModel(model);
    }

    public override void Spawn()
    {
        base.Spawn();
        SetupPhysicsFromModel(PhysicsMotionType.Static);
    }
}