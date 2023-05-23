using System.Collections.Generic;
using System;
using Sandbox;

namespace Home;

public partial class RoomLayout : BaseNetworkable
{
    [Net, Predicted] public IList<RoomLayoutEntry> Entries { get; set; } = new List<RoomLayoutEntry>();
}

public partial class RoomLayoutEntry : BaseNetworkable
{
    [Net] public string Id { get; set; }
    [Net] public Vector3 Position { get; set; }
    [Net] public Rotation Rotation { get; set; }
    [Net] public Vector3 Scale { get; set; }
    [Net] public Color Color { get; set; }
}