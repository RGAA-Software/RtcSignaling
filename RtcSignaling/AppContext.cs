using Nett;
using RtcSignaling.Room;
using RtcSignaling.User;
using Serilog;

namespace RtcSignaling;

public class AppContext
{
    private readonly ClientManager _clientManager;
    private readonly RoomManager _roomManager;
    private readonly UserDatabase _userDatabase;
    private Settings.Settings _settings;
    
    public AppContext()
    {
        _clientManager = new ClientManager(this);
        _roomManager = new RoomManager(this);
        _userDatabase = new UserDatabase(this);
    }

    public void Init()
    {
        if (!_userDatabase.Connect("mongodb://localhost:27017"))
        {
            return;
        }

        _settings = Toml.ReadFile("settings.toml").Get<Settings.Settings>();
        Log.Information("settings: " + _settings.Dump());
    }

    public ClientManager GetClientManager()
    {
        return _clientManager;
    }

    public RoomManager GetRoomManager()
    {
        return _roomManager;
    }

    public UserDatabase GetUserDatabase()
    {
        return _userDatabase;
    }

    public Settings.Settings GetSettings()
    {
        return _settings;
    }
}