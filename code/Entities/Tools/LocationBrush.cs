namespace Home;

/// <summary>
/// A brush that defines a location in the map that can be given a name to display in-game.
/// </summary>
[AutoApplyMaterial( "materials/tools/toolslocation.vmat" )]
[Library( "home_location" ), HammerEntity, Solid]
[Title( "Location Area" ), Category( "Tools" ), Icon( "place" )]
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

        if(!Game.IsServer) return;

        if ( other is HomePlayer player )
        {
            player.Location = LocationName;
        }
	}
}
