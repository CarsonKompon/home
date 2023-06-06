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
    /// The room's ID
    /// </summary>
    [Property( Title = "Room ID" )]
    public int RoomId { get; set; } = 1;


    public RoomController Room { get; set; } = null;

    public override void Spawn()
    {
        base.Spawn();

        if(Game.IsServer)
        {
            bool roomExists = false;
			for(int i=0; i<RoomController.All.Count; i++)
			{
                RoomController room = RoomController.All[i];
				if(room.Id == RoomId)
				{
					room.BuildingZones.Add(this);
                    Room = room;
					roomExists = true;
					break;
				}
			}

			// If the room does not exist, make a new one
			if(!roomExists)
			{
				Room = new RoomController(RoomId);
				Room.BuildingZones.Add(this);
			}
        }
    }

	protected override void OnTriggered( Entity other )
	{
		base.OnTriggered( other );

        if ( other is HomePlayer player )
        {
            if(Room != null && Room.State == RoomState.Vacant)
            {
                RoomFrontDoor door = Room.GetFrontDoor();
                player.Position = door.StartTransform.Position + door.StartTransform.Rotation.Backward * 64f + Vector3.Down * 24f;
            }
            else if(Game.IsServer)
            {
                player.Location = "Room #" + RoomId.ToString();
            }
        }
	}

}