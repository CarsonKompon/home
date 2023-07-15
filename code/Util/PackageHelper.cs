using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace Home.Util;

public static class PackageHelper
{
    public static async Task<Texture> GetThumbnail(string ident)
    {
        var package = await Package.FetchAsync(ident, true);
        if(package == null) return null;
        return Texture.Load(package.Thumb);
    }

    public static async Task<string> GetVideoThumbnail(string ident)
    {
        var package = await Package.FetchAsync(ident, true);
        if(package == null) return "";
        int videoId = -1;
        for(int i=0; i<package.Screenshots.Length; i++)
        {
            if(package.Screenshots[i].IsVideo)
            {
                videoId = i;
                break;
            }
        }
        if(videoId != -1) return package.Screenshots[videoId].Url;
        return (package.VideoThumb ?? package.Thumb);
    }

    public static async Task<string> GetPrimaryAsset(string ident)
    {
        var package = await Package.FetchAsync(ident, true);
        if(package == null) return "";
        return package.GetMeta("PrimaryAsset", "");
    }

}