using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace Home;

public partial class ShopFoliage : ShopBase
{
    public static ShopFoliage Instance;

    public override string Name => "Foliage Shop";
    public override string Music => "the_ruins";

    public ShopFoliage()
    {
        Instance = this;
    }

    public override List<HomePlaceable> PlaceableList()
    {
        return HomePlaceable.All.Where(x => x.Category == PlaceableCategory.Foliage && x.State == PlaceableState.Visible).ToList();
    }
    
    [ClientRpc]
    public static void Open()
    {
        if(Instance == null) Game.RootPanel.AddChild<ShopFoliage>();
        Instance.OpenLocally();
    }

}