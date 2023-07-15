using System;
using Sandbox;

namespace Home;

public partial class PetGrub : Pet
{
    public PetGrub(HomePlayer player) : base(player) {}

    public override void Spawn()
    {
        base.Spawn();

        InitModel();
    }

    async void InitModel()
    {
        var package = await Package.FetchAsync("shadb.grubterry", true);
        await package.MountAsync();
        var model = package.GetMeta("PrimaryAsset", "");
        SetModel( model );
        SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
    }

}