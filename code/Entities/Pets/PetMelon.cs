using System;
using System.Linq;
using Sandbox;
using Home.Util;

namespace Home;

[Library("pet_melon", Title = "Melon Pet", Group = "Pet")]
public partial class PetMelon : RollingPet
{

    protected override async void InitModel()
    {
        var model = await PackageHelper.GetPrimaryAsset("trend.watermelon", true);
        SetModel( model );
    }

}