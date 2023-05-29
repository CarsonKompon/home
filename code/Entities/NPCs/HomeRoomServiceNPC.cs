using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// Talking to this NPC will allow players to check-in to an available room.
/// </summary>
[Library( "home_npc_room_service" ), HammerEntity]
[EditorModel( "models/citizen/citizen.vmdl" )]
[Title( "Room Service NPC" ), Category( "NPCs" ), Icon( "person" )]
public partial class RoomServiceNPC : BaseNPC
{

    public RoomServiceNPC()
    {
        ClothingString = "[{\"id\":1594058106},{\"id\":1772984322},{\"id\":502735166},{\"id\":-1413300318},{\"id\":-1870268993},{\"id\":469696431},{\"id\":1977425295},{\"id\":-1678954621},{\"id\":-1415286990},{\"id\":-573917948},{\"id\":-1688334362}]";
    }

    public override void Spawn()
    {
        base.Spawn();
    }

    public override bool OnUse(Entity user)
    {
        if(!Game.IsServer) return false;

        if (user is HomePlayer player)
        {
            if(player.RoomNumber > -1)
            {
                // Check out of room
                HomeChatBox.AddChatEntry(To.Single(user), "", "You have checked out of room #" + player.RoomNumber.ToString(), null, "yellow");
                RoomController room = RoomController.All.Find(room => room.Id == player.RoomNumber);
                if(room != null)
                {
                    room.RemoveOwner();
                }

                return false;
            }

            if(RoomController.HasVacancies)
            {
                // Check into a room
                RoomController room = RoomController.GetOpenRoom();
                room.SetOwner(player);
                HomeChatBox.AddChatEntry(To.Single(user), "", "You have checked in to room #" + player.RoomNumber.ToString(), null, "yellow");
            }
            else
            {
                HomeChatBox.AddChatEntry(To.Single(user), "", "Room Service is currently unavailable.", null, "yellow");
            }
            
            // TODO: Open Room Service Menu
            //player.OpenRoomServiceMenu();
            
            
        }

        return false;
    }

}