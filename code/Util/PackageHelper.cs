using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sandbox;

namespace Home.Util;

public static class PackageHelper
{
    private static Dictionary<string, Texture> _Thumbnails = new();
    private static Dictionary<string, string> _VideoThumbnails = new();

    public static async Task<Texture> GetThumbnail(string ident)
    {
        if(_Thumbnails.ContainsKey(ident)) return _Thumbnails[ident];
        var package = await Package.FetchAsync(ident, true);
        if(package == null) return null;
        _Thumbnails[ident] = Texture.Load(package.Thumb);
        return _Thumbnails[ident];
    }

    public static async Task<string> GetVideoThumbnail(string ident)
    {
        if(_VideoThumbnails.ContainsKey(ident)) return _VideoThumbnails[ident];
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
        if(videoId != -1) _VideoThumbnails[ident] = package.Screenshots[videoId].Url;
        else _VideoThumbnails[ident] = (package.VideoThumb ?? package.Thumb);
        return _VideoThumbnails[ident];
    }

    public static async Task<string> GetPrimaryAsset(string ident)
    {
        var package = await Package.FetchAsync(ident, true);
        if(package == null) return "";
        return package.GetMeta("PrimaryAsset", "");
    }

}