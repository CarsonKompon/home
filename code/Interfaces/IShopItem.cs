using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sandbox;

namespace Home;

public interface IShopItem
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int Cost { get; set; }
    public string Model { get; set; }
    public string CloudIdent { get; set; }
    public string ThumbnailOverride { get; set; }

    Task<Texture> GetThumbnail();

    Task<string> GetVideoThumbnail();
}