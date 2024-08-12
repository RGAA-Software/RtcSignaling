namespace RtcSignaling;

public class HttpHandler
{
    private readonly AppContext _context;
    private readonly WebApplication _app;
    private ClientIdGenerator _idGenerator;
    
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

        _app.MapGet("/request/client/id", async context =>
        {
            string? hardware = context.Request.Query["hardware"];
            string? platform = context.Request.Query["platform"];
            var clientInfo = (platform == null ? platform:"") + hardware!;
            var id = _idGenerator.Gen(clientInfo);
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"id", id},
            }));
        });

        _app.MapGet("/check/client/online", async context =>
        {
            string clientId = context.Request.Query["client_id"]!;
            var online = _context.GetClientManager().IsClientOnline(clientId);
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"client_id", clientId},
                {"client_id_online", online},
            }));
        });

        _app.MapGet("/check/client/pair/online", async context =>
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
        });

        _app.MapGet("/online/clients", async context =>
        {
            var clients = _context.GetClientManager().GetOnlineClients();
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"clients", clients}, 
            }));
        });

        _app.MapGet("/online/rooms", async context =>
        {
            var rooms = _context.GetRoomManager().GetAllRooms();
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"rooms", rooms},
            }));
        });

        _app.MapGet("/request/room/status", async context =>
        {
            string roomId = context.Request.Query[SignalMessage.KeyRoomId]!;
            var room = _context.GetRoomManager().FindRoomById(roomId);
            if (room == null)
            {
                Response(context.Response, Errors.MakeKnownErrorMessage(Errors.ErrNoRoomFound));
                return;
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
        });
    }

    private async void Response(HttpResponse resp, string msg)
    {
        await resp.WriteAsync(msg);
    }
}