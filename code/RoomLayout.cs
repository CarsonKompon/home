namespace Home;

public partial class RoomLayout : BaseNetworkable
{
    [Net, Predicted] public string Name { get; set; } = "New Layout";
    
    public List<RoomLayoutEntry> Entries { get; set; } = new List<RoomLayoutEntry>();
}

public class RoomLayoutEntry
{
    public string Id { get; set; }
    public Vector3 Position { get; set; }
    public Rotation Rotation { get; set; }
    public float Scale { get; set; }
    public bool HasPhysics { get; set; } = false;
    public float[] Color { get; set; } = new float[] { 1f, 1f, 1f };
}
