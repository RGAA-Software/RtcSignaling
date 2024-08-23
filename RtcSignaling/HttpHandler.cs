using RtcSignaling.User;
using Serilog;

namespace RtcSignaling;

public class HttpHandler
{
    private readonly AppContext _context;
    private readonly WebApplication _app;
    private readonly ClientIdGenerator _idGenerator;
    
    public HttpHandler(AppContext ctx, WebApplication app)
    {
        _context = ctx;
        _app = app;
        _idGenerator = new ClientIdGenerator();
    }

    public void RegisterHandlers()
    {
        _app.MapGet("/ping", () => 
        {
            return Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"message", "pong"},
            }); 
        }).WithName("ping");

        _app.MapGet("/request/client/id", context =>
        {
            string? hardware = context.Request.Query["hardware"];
            string? platform = context.Request.Query["platform"];
            var clientInfo = (platform == null ? platform:"") + hardware!;
            
            if (hardware == null || hardware.Length <= 0)
            {
                Log.Error("hardware info invalid");
                Response(context.Response, Errors.MakeKnownErrorMessage(Errors.ErrInvalidParam));
                return Task.CompletedTask;
            }

            var targetId = "";
            while (true)
            {
                // query user by hardware information
                var user = _context.GetUserDatabase().FindUserByClientInfo(clientInfo);
                // user exists
                if (user != null)
                {
                    targetId = user.Uid;
                    break;
                }
                
                // generate user id
                targetId = _idGenerator.Gen(clientInfo);
                // query user by new id
                user = _context.GetUserDatabase().FindUserById(targetId);
                // already exists, generate a new one
                if (user != null)
                {
                    continue;
                }

                _context.GetUserDatabase().SaveUser(new User.User
                {
                    Uid = targetId,
                    ClientInfo = clientInfo,
                    CreateTimestamp = Common.GetCurrentTimestamp(),
                    LastModifyTimestamp = Common.GetCurrentTimestamp(),
                });
                break;
            }

            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {SignalMessage.KeyId, targetId},
            }));
            return Task.CompletedTask;
        });

        _app.MapGet("/check/client/online", context =>
        {
            string clientId = context.Request.Query["client_id"]!;
            var online = _context.GetClientManager().IsClientOnline(clientId);
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"client_id", clientId},
                {"client_id_online", online},
            }));
            return Task.CompletedTask;
        });

        _app.MapGet("/check/client/pair/online", context =>
        {
            string clientId = context.Request.Query["client_id"]!;
            var clientIdOnline = _context.GetClientManager().IsClientOnline(clientId!);
            string remoteClientId = context.Request.Query["remote_client_id"]!;
            var remoteClientIdOnline = _context.GetClientManager().IsClientOnline(remoteClientId);
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"client_id", clientId},
                {"client_id_online", clientIdOnline},
                {"remote_client_id", remoteClientId},
                {"remote_client_id_online", remoteClientIdOnline},
            }));
            return Task.CompletedTask;
        });

        _app.MapGet("/online/clients", context =>
        {
            var clients = _context.GetClientManager().GetOnlineClients();
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"clients", clients}, 
            }));
            return Task.CompletedTask;
        });

        _app.MapGet("/online/rooms", context =>
        {
            var rooms = _context.GetRoomManager().GetAllRooms();
            
            var targetRooms = new List<Dictionary<string, object>>();
            foreach (var room in rooms)
            {
                targetRooms.Add(new Dictionary<string, Object>
                {
                    {SignalMessage.KeyId, room.Id},
                    {SignalMessage.KeyName, room.Name},
                    {SignalMessage.KeyClients, room.GetClients()},    
                });    
            }
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"rooms", targetRooms},
            }));
            return Task.CompletedTask;
        });

        _app.MapGet("/request/room/status", context =>
        {
            string roomId = context.Request.Query[SignalMessage.KeyRoomId]!;
            var room = _context.GetRoomManager().FindRoomById(roomId);
            if (room == null)
            {
                Response(context.Response, Errors.MakeKnownErrorMessage(Errors.ErrNoRoomFound));
                return Task.CompletedTask;
            }
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>()
            {
                {
                    SignalMessage.KeyRoom, new Dictionary<string, Object>
                    {
                        {SignalMessage.KeyId, roomId},
                        {SignalMessage.KeyName, room.Name},
                        {SignalMessage.KeyClients, room.GetClients()},    
                    }
                },
            }));
            return Task.CompletedTask;
        });

        _app.MapGet("/total/users", context =>
        {
            var count = _context.GetUserDatabase().GetTotalUsers();
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"count", count},
            }));
            return Task.CompletedTask;
        });
    }

    private static async void Response(HttpResponse resp, string msg)
    {
        await resp.WriteAsync(msg);
    }
}