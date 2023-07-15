using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;
using Home.Util;

namespace Home;

[GameResource("Home Pet", "pet", "Describes a Home pet.", Icon = "pets" )]
public partial class HomePet : GameResource, IShopItem
{
    public string Name { get; set; } = "Missingname.";
    public string Description { get; set; } = "";
    public int Cost { get; set; } = 0;

    [ResourceType("vmdl")]
    public string Model { get; set; }
    public string ClassName { get; set; } = "";
    public string CloudIdent { get; set; } = "";

    [ResourceType("png")]
    public string ThumbnailOverride { get; set; } = "";

    public async Task<Texture> GetThumbnail()
    {
        if(string.IsNullOrEmpty(ThumbnailOverride))
        {
            if(!string.IsNullOrEmpty(CloudIdent))
            {
                return await PackageHelper.GetThumbnail(CloudIdent);

            }
        }
        
        return Texture.Load(FileSystem.Mounted, ThumbnailOverride);
    }

    private string _VideoThumbnail = "";
    public async Task<string> GetVideoThumbnail()
    {
        if(!string.IsNullOrEmpty(_VideoThumbnail)) return _VideoThumbnail;
        return await PackageHelper.GetVideoThumbnail(CloudIdent);
    }

    public static List<HomePet> All => ResourceLibrary.GetAll<HomePet>().ToList();

    public static HomePet Find(int id)
    {
        return All.Find(p => p.ResourceId == id);
    }
}