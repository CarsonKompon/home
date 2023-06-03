using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Home;

public class ShopFurniture : ShopBase
{
    public static ShopFurniture Instance;

    public ShopFurniture()
    {
        Instance = this;
    }

    public override List<HomePlaceable> PlaceableList()
    {
        return HomePlaceable.All.ToList();
    }
    
}