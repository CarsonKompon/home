using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[AutoApplyMaterial( "materials/tools/toolslocation.vmat" )]
[Library( "home_location" ), HammerEntity, Solid]
[Title( "Home Location Area" ), Category( "Home" ), Icon( "house" )]
public class LocationBrush : TriggerMultiple
{
    /// <summary>
    /// The name of this location
    /// </summary>
    [Property( Title = "Location Name" )]
    public string LocationName { get; set; } = "Unnamed Location";

	protected override void OnTriggered( Entity other )
	{
		base.OnTriggered( other );

        if ( other is HomePlayer player )
        {
            player.Location = LocationName;
        }
	}
}