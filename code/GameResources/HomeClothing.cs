using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using Sandbox;
using Home.Util;

namespace Home;


[GameResource("Home Clothing", "hcloth", "Describes a Home clothing article.", Icon = "dry_cleaning" )]
public partial class HomeClothing : Clothing
{
    
	//
	// Summary:
	//     The cost of the clothing article.
	public int Cost { get; set; } = 0;

	//
	// Summary:
	//     The model to bonemerge to the player when this clothing is equipped.
	[Category( "Clothing Setup" )]
	public string CloudModel { get; set; } = "";

	private Model _Model = null;

	protected override void PostLoad()
	{
		base.PostLoad();
	}

	public async Task MountPackage()
	{
		Log.Info($"Mounting clothing from {CloudModel}");
		if(string.IsNullOrEmpty(CloudModel))
		{
			Log.Error($"No cloud model for {this}");
			return;
		}
		Package package = await Package.FetchAsync(CloudModel, true);
		try
		{
			await package.MountAsync();
			Model = package.GetMeta("PrimaryAsset", "");
			Log.Info($"Mounted clothing from {CloudModel} as {Model}");
		}
		catch(Exception e)
		{
			Log.Info($"Failed to mount clothing from {CloudModel}:");
			Log.Info(e);
		}
	}

	// public static string GetModel(Clothing clothing)
	// {
	// 	if(clothing is HomeClothing hcloth)
	// 	{
	// 		if ( string.IsNullOrEmpty(hcloth._ModelPath) )
	// 		{
	// 			if(string.IsNullOrEmpty(hcloth.CloudModel))
	// 			{
	// 				hcloth._ModelPath = hcloth.Model;
	// 			}
	// 			else
	// 			{
	// 				hcloth._Model = Cloud.Model(hcloth.CloudModel);
	// 				hcloth._ModelPath = hcloth._Model.ResourcePath;
	// 			}
	// 		}

	// 		return hcloth._ModelPath;
	// 	}
		
	// 	return clothing.Model;
	// }

	public static async Task<Texture> GetIcon(Clothing clothing)
	{
		if(clothing.Icon.Path != null && FileSystem.Mounted.FileExists(clothing.Icon.Path))
		{
			return Texture.Load(FileSystem.Mounted, clothing.Icon.Path);
		}
		
		if(clothing is HomeClothing hcloth && !string.IsNullOrEmpty(hcloth.CloudModel))
		{
			Log.Info($"Getting cloud thumbnail for {hcloth.CloudModel}");
			var package = await Package.FetchAsync(hcloth.CloudModel, true);
			return Texture.Load(package.Thumb);
		}

		return SceneHelper.CreateClothingThumbnail( clothing );
	}

	string _VideoThumbnail = null;
	public async Task<string> GetVideoThumbnail()
    {
        if(!string.IsNullOrEmpty(_VideoThumbnail)) return _VideoThumbnail;
        if(string.IsNullOrEmpty(CloudModel)) return "";
		Log.Info($"Getting cloud video for {CloudModel}");
        var package = await Package.FetchAsync(CloudModel, true);
        int videoId = -1;
        for(int i=0; i<package.Screenshots.Length; i++)
        {
            if(package.Screenshots[i].IsVideo)
            {
                videoId = i;
                break;
            }
        }
        if(videoId != -1) _VideoThumbnail = package.Screenshots[videoId].Url;
        else _VideoThumbnail = (package.VideoThumb ?? package.Thumb);
        return _VideoThumbnail;
    }


    public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();

	public static List<HomeClothing> AllHome => ResourceLibrary.GetAll<HomeClothing>().ToList();

	public static List<Clothing> GetAll(HomePlayer player)
	{
		var list = ResourceLibrary.GetAll<Clothing>().Where(x => x is not HomeClothing).ToList();
		foreach(var clothing in AllHome)
		{
			if(player.Data.Clothing.Contains(clothing.ResourceId))
			{
				list.Add(clothing);
			}
		}
		return list;
	}

    //public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();
}
