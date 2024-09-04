using Serilog;

namespace RtcSignaling.Room;

public class RoomManager(AppContext ctx)
{
    private AppContext _context = ctx;
    private readonly object _roomsMutex = new object();
    private readonly Dictionary<string, Room> _rooms = new();

    private static string GenRoomId(string token, string clientId, string remoteClientId)
    {
        //return Common.Md5String(remoteClientId);
        return clientId + "-" + remoteClientId;
    }

    public Room CreateRoom(string token, string clientId, string remoteClientId, string groupId)
    {
        var targetRoomId = GenRoomId(token, clientId, remoteClientId);
        lock (_roomsMutex)
        {
            foreach (var pair in _rooms)
            {
                var roomId = pair.Key;
                var room = pair.Value;
                if (roomId == targetRoomId)
                {
                    room.Creator = clientId;
                    room.GroupId = groupId;
                    return room;
                }
            }
            var newRoom = new RtcSignaling.Room.Room
            {
                Id = targetRoomId,
                Creator = clientId,
                GroupId = groupId,
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

    public List<Room> GetActiveRoomsByGroupId(string groupId)
    {
        lock (_roomsMutex)
        {
            var rooms = new List<Room>();
            foreach (var pair in _rooms)
            {
                var room = pair.Value;
                if (room.GroupId == groupId && room.HasRemotePeer())
                {
                    rooms.Add(room);
                }
            }
            return rooms;
        }
    }

    public int CountActiveRoomsByGroupId(string groupId)
    {
        return GetActiveRoomsByGroupId(groupId).Count;
    }

    public Dictionary<string, List<Room>> GetActiveRoomsByGroupIdAndClassifyByClientId(string groupId)
    {
        var result = new Dictionary<string, List<Room>>();
        var rooms = GetActiveRoomsByGroupId(groupId);
        foreach (var room in rooms)
        {
            var creatorClientId = room.Creator;
            if (!result.ContainsKey(creatorClientId))
            {
                result.Add(creatorClientId, new List<Room>());
            }
            result[creatorClientId].Add(room);
        }

        return result;
    }

    public int CountActiveRoomsByGroupIdAndClassifyByClientId(string groupId)
    {
        return GetActiveRoomsByGroupIdAndClassifyByClientId(groupId).Count;
    }

    public List<Room> GetRoomsCreatedByClientId(string clientId)
    {
        var rooms = new List<Room>();
        lock (_roomsMutex)
        {
            foreach (var pair in _rooms)
            {
                var room = pair.Value;
                if (room.Creator == clientId)
                {
                    rooms.Add(room);
                }
            }
        }

        return rooms;
    }
}