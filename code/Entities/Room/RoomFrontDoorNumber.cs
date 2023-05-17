using Sandbox.UI;
using Sandbox.UI.Construct;
using Editor;

namespace Home;


public class RoomFrontDoorNumber : WorldPanel
{
    public Label NumberLabel;

    public RoomFrontDoorNumber()
    {
        StyleSheet.Load("/Entities/Room/RoomFrontDoorNumber.scss");
        NumberLabel = Add.Label("1", "title");
    }

    public void SetState(RoomState state)
    {
        RemoveClass("vacant");
        RemoveClass("open");
        RemoveClass("locked");
        RemoveClass("friends-only");

        switch ( state )
        {
            case RoomState.Vacant:
                AddClass("vacant");
                break;
            case RoomState.Open:
                AddClass("open");
                break;
            case RoomState.Locked:
                AddClass("locked");
                break;
            case RoomState.FriendsOnly:
                AddClass("friends-only");
                break;
        }
    }

}