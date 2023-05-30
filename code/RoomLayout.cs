using System.Collections.Generic;
using System.Text.Json;
using Sandbox;

namespace Home;

public partial class RoomLayout : BaseNetworkable
{
    [Net, Predicted] public string Name { get; set; } = "New Layout";
    
    public List<RoomLayoutEntry> Entries { get; set; } = new List<RoomLayoutEntry>();
}

public partial class RoomLayoutEntry : BaseNetworkable
{
    [Net] public string Id { get; set; }
    [Net] public Vector3 Position { get; set; }
    [Net] public Rotation Rotation { get; set; }
    [Net] public float Scale { get; set; }
}