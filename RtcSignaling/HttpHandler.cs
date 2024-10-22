using RtcSignaling.Password;
using RtcSignaling.User;
using Serilog;

namespace RtcSignaling;

public class HttpHandler : BaseHttpHandler
{
    private readonly ClientIdGenerator _idGenerator = new();
    
    public HttpHandler(AppContext ctx, WebApplication app) : base(ctx, app)
    {
        this.RegisterHandlers();
    }

    protected sealed override void RegisterHandlers() 
    {
        App.MapGet(Api.ApiPing, context =>
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var port = context.Connection.RemotePort;
            string? clientTsStr = context.Request.Query["client_ts"];
            var clientTs = clientTsStr ?? "";
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"message", "pong"},
                {"www_ip", ip ?? ""},
                {"www_port", port},
                {"client_ts", clientTs},
            }));
            return Task.CompletedTask;
        });

        App.MapGet(Api.ApiRequestClientId, context =>
        {
            string? hardware = context.Request.Query["hardware"];
            string? platform = context.Request.Query["platform"];
            var clientInfo = hardware!;
            
            if (hardware == null || hardware.Length <= 0)
            {
                Log.Error("hardware info invalid");
                Response(context.Response, Errors.MakeKnownErrorMessage(Errors.ErrInvalidParam));
                return Task.CompletedTask;
            }

            var targetId = "";
            var randomPwd = "";
            var db = Context.GetUserDatabase();
            var fixedIndices = true;
            while (true)
            {   
                // query user by hardware information
                var user = db.FindUserByClientInfo(clientInfo);
                // user exists
                if (user != null)
                {
                    var newRandomPwd = RandomPwdGenerator.GenRandomPassword();
                    if (!db.UpdateRandomPwd(user.Uid, Common.Md5String(newRandomPwd)))
                    {
                        ResponseDbError(context);
                        return Task.CompletedTask;
                    }
                    targetId = user.Uid;
                    randomPwd = newRandomPwd;
                    break;
                }
                
                // generate user id
                targetId = _idGenerator.Gen(clientInfo, fixedIndices);
                Log.Information("GEN, targetId: " + targetId + ", clientInfo: " + clientInfo + ", fixedIndices: " + fixedIndices);
                // query user by new id
                user = Context.GetUserDatabase().FindUserById(targetId);
                // already exists, generate a new one
                if (user != null)
                {
                    fixedIndices = false;
                    continue;
                }

                randomPwd = RandomPwdGenerator.GenRandomPassword();
                db.SaveUser(new User.User
                {
                    Uid = targetId,
                    ClientInfo = clientInfo,
                    CreateTimestamp = Common.GetCurrentTimestamp(),
                    LastModifyTimestamp = Common.GetCurrentTimestamp(),
                    RandomPwd = Common.Md5String(randomPwd),
                });
                break;
            }

            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {SignalMessage.KeyId, targetId},
                {SignalMessage.KeyRandomPwd, randomPwd},
            }));
            return Task.CompletedTask;
        });

        App.MapGet(Api.ApiCheckClientOnline, context =>
        {
            string clientId = context.Request.Query["client_id"]!;
            var client = Context.GetClientManager().GetOnlineClientById(clientId);
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"client_id", clientId},
                {"client_id_online", client != null},
                {"client", client ?? new Client()},
            }));
            return Task.CompletedTask;
        });

        App.MapGet(Api.ApiCheckClientPairOnline, context =>
        {
            string clientId = context.Request.Query["client_id"]!;
            var clientIdOnline = Context.GetClientManager().IsClientOnline(clientId!);
            string remoteClientId = context.Request.Query["remote_client_id"]!;
            var remoteClientIdOnline = Context.GetClientManager().IsClientOnline(remoteClientId);
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"client_id", clientId},
                {"client_id_online", clientIdOnline},
                {"remote_client_id", remoteClientId},
                {"remote_client_id_online", remoteClientIdOnline},
            }));
            return Task.CompletedTask;
        });

        App.MapGet(Api.ApiOnlineClients, context =>
        {
            var clients = Context.GetClientManager().GetOnlineClients();
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"clients", clients}, 
            }));
            return Task.CompletedTask;
        });

        App.MapGet(Api.ApiOnlineRooms, context =>
        {
            var rooms = Context.GetRoomManager().GetAllRooms();
            
            var targetRooms = new List<Dictionary<string, object>>();
            foreach (var room in rooms)
            {
                targetRooms.Add(new Dictionary<string, Object>
                {
                    {SignalMessage.KeyId, room.Id},
                    {SignalMessage.KeyName, room.Name},
                    {SignalMessage.KeyGroupId, room.GroupId},
                    {SignalMessage.KeyCreator, room.Creator},
                    {SignalMessage.KeyClients, room.GetClients()},    
                });    
            }
            Response(context.Response, Common.MakeOkJsonMessage(new Dictionary<string, object>
            {
                {"rooms", targetRooms},
            }));
            return Task.CompletedTask;
        });

        App.MapGet(Api.ApiRequestRoomStatus, context =>
        {
            string roomId = context.Request.Query[SignalMessage.KeyRoomId]!;
            var room = Context.GetRoomManager().FindRoomById(roomId);
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
    }

    private static async void Response(HttpResponse resp, string msg)
    {
        await resp.WriteAsync(msg);
    }
}