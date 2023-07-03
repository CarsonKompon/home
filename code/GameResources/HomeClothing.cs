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

    public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();

    //public static List<Clothing> All => ResourceLibrary.GetAll<Clothing>().ToList();
}
