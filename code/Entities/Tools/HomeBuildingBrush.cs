using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[AutoApplyMaterial( "materials/tools/toolsroom.vmat" )]
[Library( "home_room" ), HammerEntity, Solid]
[Title( "Room Building Zone" ), Category( "Home" ), Icon( "house" )]
public class RoomBuildingZone : TriggerMultiple
{
    /// <summary>
    /// The name of this location
    /// </summary>
    [Property( Title = "Room ID" )]
    public int RoomId { get; set; } = 1;

    protected override void OnTriggered( Entity other )
	{
		base.OnTriggered( other );

        if ( other is HomePlayer player )
        {
            player.Location = "Room #" + RoomId.ToString();
        }
	}

}