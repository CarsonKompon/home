using System;
using Sandbox;
using Home.Util;

namespace Home;

[Library("pet_skibidi", Title = "Skibidi Terry Pet", Group = "Pet")]
public partial class PetSkibidi : Pet
{
    Sound Music;
    bool IsDressed = false;

    public override void Spawn()
    {
        base.Spawn();

        InitModel();

        Scale = 0.5f;

        Music = Sound.FromEntity("skibidi_terry", this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Music.Stop();
    }

    async void InitModel()
    {
        var package = await Package.FetchAsync("turd.terry_skibidi", true);
        await package.MountAsync();
        var model = package.GetMeta("PrimaryAsset", "");
        SetModel( model );
        SetupPhysicsFromModel( PhysicsMotionType.Keyframed, false );
    }

    protected override void TickAnimation()
    {
        if(State != PetState.Idle)
        {
            // Set Rotation from PreviousVelocity
            Random random = new Random();
            Rotation = Rotation.LookAt( PreviousVelocity.WithZ( 0 ), Vector3.Up ).RotateAroundAxis(Vector3.Up, random.Float(-2f, 2f));
        }
    }
}