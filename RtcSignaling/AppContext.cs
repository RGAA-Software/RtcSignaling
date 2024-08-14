namespace RtcSignaling;

public class AppContext
{
    private readonly ClientManager _clientManager;
    private readonly RoomManager _roomManager;

    public AppContext()
    {
        _clientManager = new ClientManager(this);
        _roomManager = new RoomManager(this);
    }

    public void Init()
    {
    }

    public ClientManager GetClientManager()
    {
        return _clientManager;
    }

    public RoomManager GetRoomManager()
    {
        return _roomManager;
    }
}