using Serilog;

namespace RtcSignaling;

public class RoomManager(AppContext ctx)
{
    private AppContext _context = ctx;
    private readonly object _roomsMutex = new object();
    private readonly Dictionary<string, Room> _rooms = new Dictionary<string, Room>();

    private static string GenRoomId(string token, string clientId, string remoteClientId)
    {
        //return Common.Md5String(remoteClientId);
        return clientId + "-" + remoteClientId;
    }

    public Room CreateRoom(string token, string clientId, string remoteClientId)
    {
        var targetRoomId = GenRoomId(token, clientId, remoteClientId);
        lock (_roomsMutex)
        {
            foreach (var pair in _rooms)
            {
                var roomId = pair.Key;
                if (roomId == targetRoomId)
                {
                    return pair.Value;
                }
            }
            var newRoom = new Room
            {
                Id = targetRoomId,
            };
            _rooms[targetRoomId] = newRoom;
            return newRoom;
        }
    }

    public Room? FindRoomById(string roomId)
    {
        lock (_roomsMutex)
        {
            foreach (var pair in _rooms)
            {
                if (pair.Value.Id == roomId)
                {
                    return pair.Value;
                }
            }
        }
        return null;
    }

    public void RemoveClientInRoom(string roomId, string clientId)
    {
        lock (_roomsMutex)
        {
            foreach (var pair in _rooms)
            {
                var room = pair.Value;
                if (room.Id == roomId)
                {
                    room.RemoveClient(clientId);
                    break;
                }
            }
        }
    }

    public List<Room> GetAllRooms()
    {
        lock (_roomsMutex)
        {
            var rooms = new List<Room>();
            foreach (var pair in _rooms)
            {
                rooms.Add(pair.Value);
            }
            return rooms;
        }
    }

    public void RemoveRoomById(string roomId)
    {
        lock (_roomsMutex)
        {
            _rooms.Remove(roomId);
        }
    }

    public void CleanEmptyRooms()
    {
        lock (_roomsMutex)
        {
            var toRemoveRooms = new List<string>();
            foreach (var pair in _rooms)
            {
                var room = pair.Value;
                if (room.IsAlreadyUsed() && room.Empty())
                {
                    toRemoveRooms.Add(pair.Key);
                }
            }
            foreach (var roomId in toRemoveRooms)
            {
                _rooms.Remove(roomId);
                Log.Information("Clean room:" + roomId);
            }
        }
    }
}