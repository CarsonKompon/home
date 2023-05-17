using Sandbox;
using Editor;

namespace Home;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>
[Library( "home_front_door" ), HammerEntity, SupportsSolid]
[Model( Archetypes = ModelArchetype.animated_model )]
[DoorHelper( "movedir", "movedir_islocal", "movedir_type", "distance" )]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Title( "Room Front Door" ), Category( "Room" ), Icon( "house" )]
public partial class RoomFrontDoor : DoorEntity
{
    /// <summary>
    /// The name of this location
    /// </summary>
    [Property( Title = "Room ID" )]
    [Net] public int RoomId { get; set; } = 1;

    public RoomFrontDoorNumber Number;

    public override void Spawn()
    {
        base.Spawn();

        if(Game.IsServer)
        {
            SetState(RoomState.Vacant);
        }
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();

        Number = new RoomFrontDoorNumber();
        Number.NumberLabel.Text = RoomId.ToString();
        SetNumberState(RoomState.Vacant);
    }

    [GameEvent.Client.Frame]
    void Tick()
    {
        if ( Number != null )
        {
            Number.Position = Position + Rotation.Backward * 2.125f + Rotation.Right * 25f;
            Number.Rotation = Rotation * Rotation.From( new Angles( 0, 180, 0 ) );
        }
    }

    public void SetState(RoomState state)
    {
        switch ( state )
        {
            case RoomState.Vacant:
                Lock();
                break;
            case RoomState.Open:
                Unlock();
                break;
            case RoomState.Locked:
                Lock();
                break;
            case RoomState.FriendsOnly:
                Unlock();
                break;
        }

        SetNumberState(state);
    }

    [ClientRpc]
    public void SetNumberState(RoomState state)
    {
        Number?.SetState(state);
    }

}