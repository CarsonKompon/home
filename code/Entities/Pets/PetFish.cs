using System;
using System.Linq;
using Sandbox;
using Home.Util;

namespace Home;

[Library("pet_fish", Title = "Fish Pet", Group = "Pet")]
public partial class PetFish : RollingPet
{

    protected override async void InitModel()
    {
        var model = await PackageHelper.GetPrimaryAsset("fish.pike", true);
        SetModel( model );

        Scale = 2f;
    }

}