using Sandbox;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[AutoApplyMaterial( "materials/tools/toolsroom.vmat" )]
[Library( "home_room" ), HammerEntity, Solid]
[Title( "Room Building Zone" ), Category( "Room" ), Icon( "house" )]
public partial class RoomBuildingZone : TriggerMultiple
{
    /// <summary>
    /// The name of this location
    /// </summary>
    [Property( Title = "Room ID" )]
    public int RoomId { get; set; } = 1;


    public HomePlayer OwningPlayer { get; set; } = null;

    public DoorEntity FrontDoor = null;
    public bool IsOwned => OwningPlayer != null;

    public override void Spawn()
    {
        base.Spawn();

        if(Game.IsServer)
        {
            bool roomExists = false;
			for(int i=0; i<RoomController.All.Count; i++)
			{
				if(RoomController.All[i].Id == RoomId)
				{
					RoomController.All[i].BuildingZones.Add(this);
					roomExists = true;
					break;
				}
			}

			// If the room does not exist, make a new one
			if(!roomExists)
			{
				RoomController room = new RoomController();
				room.Id = RoomId;
				room.BuildingZones.Add(this);

				// Find the front door for the room
				Entity.All.OfType<RoomFrontDoor>().ToList().ForEach(frontDoor => {
					if(frontDoor.RoomId == room.Id)
					{
						room.FrontDoor = frontDoor;
					}
				});
			}
        }
    }

	protected override void OnTriggered( Entity other )
	{
		base.OnTriggered( other );

        if ( other is HomePlayer player )
        {
            player.Location = "Room #" + RoomId.ToString();
        }
	}

}