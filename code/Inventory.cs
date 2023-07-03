namespace Home;

public partial class Inventory
{
    public Entity Owner {get; init; }
    public List<Entity> List = new List<Entity>();
    public virtual Entity Active
    {
        get
        {
            return (Owner as HomePlayer)?.ActiveChild;
        }

        set
        {
            if(Owner is HomePlayer player)
            {
                player.ActiveChild = value;
            }
        }
    }

    public Inventory(Entity owner)
    {
        Owner = owner;
    }
}
