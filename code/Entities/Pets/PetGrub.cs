using System;
using Sandbox;
using Home.Util;

namespace Home;

public partial class PetGrub : Pet
{
    bool IsDressed = false;

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

        DressFromString(Player.ClothingString);
    }

    protected override void TickAnimation()
    {
        if(!IsDressed)
        {
            if(Player.ClothingString != "")
            {
                DressFromString(Player.ClothingString);
                IsDressed = true;
            }
        }

        SetAnimParameter( "grounded", true );
        SetAnimParameter( "velocity", PreviousVelocity.Length);

        if(State != PetState.Idle)
        {
            // Set Rotation from PreviousVelocity
            Rotation = Rotation.LookAt( PreviousVelocity.WithZ( 0 ), Vector3.Up );
        }
    }

    public override async void DressFromString(string clothingString)
    {
        foreach(var child in Children)
        {
            if(child.Tags.Has("clothes"))
            {
                child.Delete();
            }
        }

        var clothes = new ClothingContainer();
        clothes.Deserialize(clothingString);

        for(int i=0; i<clothes.Clothing.Count; i++)
        {
            var item = clothes.Clothing[i];
            if(item.Category == Clothing.ClothingCategory.Tops ||
               item.Category == Clothing.ClothingCategory.Bottoms ||
               item.Category == Clothing.ClothingCategory.Footwear)
            {
                clothes.Clothing.RemoveAt(i);
                i--;
            }
        }

        foreach(var item in clothes.Clothing)
		{
			if(item is HomeClothing hcloth && !string.IsNullOrEmpty(hcloth.CloudModel))
			{
				await hcloth.MountPackage();
			}
		}

        ClothingHelper.DressEntity(this, clothes);
        
        // foreach(var item in clothes.Clothing)
        // {
        //     var ent = new AnimatedEntity(item.Model, this);
                
        //     ent.Tags.Add("clothes");

        //     if(!string.IsNullOrEmpty(item.MaterialGroup))
        //         ent.SetMaterialGroup(item.MaterialGroup);

        //     if(item.Category != Clothing.ClothingCategory.Skin)
        //         continue;

        //     if(item.Model != null)
        //     {
        //         if(item.ResourceName.ToLower().Contains("skel"))
        //         {
        //             // TODO: Support Grub Skeleton
        //             continue;
        //         }

        //         var materials = Model.Load(item.Model).Materials;

        //         var skinMaterial = Material.Load("models/citizen/skin/citizen_skin01.vmat");
        //         var eyeMaterial = Material.Load( "models/citizen/skin/citizen_eyes_advanced.vmat" );
        //         foreach ( var mat in materials )
        //         {
        //             if ( mat.Name.Contains( "eyes" ) )
        //                 eyeMaterial = mat;

        //             if ( mat.Name.Contains( "_skin" ) )
        //                 skinMaterial = mat;
        //         }

        //         SetMaterialOverride( skinMaterial, "skin" );
        //         SetMaterialOverride( eyeMaterial, "eyes" );
        //     }
        //     else
        //     {
        //         var skinMaterial = Material.Load(item.SkinMaterial);
        //         SetMaterialOverride( skinMaterial, "skin" );
        //     }
        // }
    }

}