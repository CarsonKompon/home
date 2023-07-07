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

	public static List<HomeClothing> LoadingQueue = new List<HomeClothing>();

	protected override void PostLoad()
	{
		base.PostLoad();
		if (!string.IsNullOrEmpty(CloudModel) )
		{
			//LoadingQueue.Add(this);
			// _Model = Cloud.Model(CloudModel);
			// Model = _Model.ResourcePath;

			MountPackage();
		}
	}

	async void MountPackage()
	{
		Package package = await Package.FetchAsync(CloudModel, false);
		await package.MountAsync();
		Model = package.GetMeta("PrimaryAsset", "");
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

	public static Texture GetIcon(Clothing clothing)
	{
		if(clothing.Icon.Path != null && FileSystem.Mounted.FileExists(clothing.Icon.Path))
		{
			return Texture.Load(FileSystem.Mounted, clothing.Icon.Path);
		}
		else
		{
			return SceneHelper.CreateClothingThumbnail( clothing );
		}
	}

	string _VideoThumbnail = null;
	public async Task<string> GetVideoThumbnail()
    {
        if(!string.IsNullOrEmpty(_VideoThumbnail)) return _VideoThumbnail;
        if(string.IsNullOrEmpty(CloudModel)) return "";
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

    //public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();
}
