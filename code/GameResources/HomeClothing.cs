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
			LoadingQueue.Add(this);
			// _Model = Cloud.Model(CloudModel);
			// Model = _Model.ResourcePath;
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


    public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();

    //public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();
}
