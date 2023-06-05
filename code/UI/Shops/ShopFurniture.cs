using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Home;

public partial class ShopFurniture : ShopBase
{
    public static ShopFurniture Instance;

    public override string Name => "Furniture Shop";

    public ShopFurniture()
    {
        Instance = this;
    }

    public override List<HomePlaceable> PlaceableList()
    {
        return HomePlaceable.All.Where(x => x.Category == PlaceableCategory.Furniture && x.State == PlaceableState.Visible).ToList();
    }

    [ClientRpc]
    public static void Open()
    {
        if(Instance == null) Game.RootPanel.AddChild<ShopFurniture>();
        Instance.OpenLocally();
    }
    
}