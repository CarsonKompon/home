using Sandbox;
using System.Collections.Generic;

namespace ArcadeZone;

partial class Inventory
{
    public Entity Owner {get; init; }
    public List<Entity> List = new List<Entity>();
    public virtual Entity Active
    {
        get
        {
            return (Owner as AZPlayer)?.ActiveChild;
        }

        set
        {
            if(Owner is AZPlayer player)
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