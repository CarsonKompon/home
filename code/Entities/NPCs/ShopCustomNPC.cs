namespace Home;

/// <summary>
/// Talking to this NPC will open a shop containing placeables tagged with whatever you choose, dressed however you like.
/// </summary>
[Library( "home_npc_shop_custom" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Custom Shop NPC" ), Category( "NPCs" ), Icon( "person_add" )]
public partial class ShopCustomNPC : BaseNPC
{

    /// <summary>
    /// The name of this Shop/NPC.
    /// </summary>
    [Property( Title = "Display Name" )]
    private string displayName {get; set;} = "Custom Shop";

    /// <summary>
    /// The tag to filter shop items by.
    /// </summary>
    [Property( Title = "Shop Tag" )]
    public string ShopTag {get; set;} = "furniture";

    /// <summary>
    /// The Serialized JSON string of the clothing to dress this NPC in.
    /// </summary>
    [Property( Title = "Clothing String" )]
    private string clothingString {get; set;} = "[]";



    public override string DisplayName => displayName;

    protected override string ClothingString => clothingString;
    

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        ShopPlaceable.Open(To.Single(user), ShopTag, DisplayName);


        return false;
    }

}
