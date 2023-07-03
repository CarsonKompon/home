namespace Home;

public class HomeLayout
{
    public string Name { get; set; }
    public LayoutObject[] Objects { get; set; }
}

public class LayoutObject
{
    public string Id { get; set; }
    public Vector3 Position { get; set; }
    public Rotation Rotation { get; set; }
}
