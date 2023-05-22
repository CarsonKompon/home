using Sandbox;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[Library( "home_room_editable_material" ), HammerEntity, Solid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Title( "Room Editable Material" ), Category( "Room" ), Icon( "wallpaper" )]
public partial class RoomEditableMaterial : BrushEntity
{
    /// <summary>
    /// The name of this location
    /// </summary>
    [Property( Title = "Room ID" )]
    public int RoomId { get; set; } = 1;

    /// <summary>
    /// The name of this editable material
    /// </summary>
    [Property( Title = "Editable Material Name" )]
    public string EditableMaterialName { get; set; } = "Unnamed Wall";

    public RoomController Room { get; set; } = null;

    public override void Spawn()
    {
        base.Spawn();

        SetupPhysicsFromModel( PhysicsMotionType.Static );

        if(Game.IsServer)
        {
            bool roomExists = false;
			for(int i=0; i<RoomController.All.Count; i++)
			{
                RoomController room = RoomController.All[i];
				if(room.Id == RoomId)
				{
					room.EditableMaterials.Add(this);
                    Room = room;
					roomExists = true;
					break;
				}
			}

			// If the room does not exist, make a new one
			if(!roomExists)
			{
				Room = new RoomController(RoomId);
				Room.EditableMaterials.Add(this);
			}
        }
    }

}