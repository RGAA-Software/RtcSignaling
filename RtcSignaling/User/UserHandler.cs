using Serilog;

namespace RtcSignaling.User;

public class UserHandler : BaseHttpHandler
{
    public UserHandler(AppContext ctx, WebApplication app) : base(ctx, app)
    {
        RegisterHandlers();
    }

    protected sealed override void RegisterHandlers()
    {
        HandleTotalUsers();
        HandleUserInfo();
        HandleVerifyUserInfo();
        HandleSendMessage();
    }

    private void HandleTotalUsers()
    {
        App.MapGet(Api.ApiTotalUsers, context =>
        {
            var count = Context.GetUserDatabase().GetTotalUsers();
            ResponseOk(context, new Dictionary<string, object>
            {
                {"count", count},
            });
            return Task.CompletedTask;
        });
    }

    // 读取数据库里的内容
    private void HandleUserInfo()
    {
        App.MapGet(Api.ApiRequestUserInfo, context =>
        {
            string? id = context.Request.Query[SignalMessage.KeyClientId];
            if (Common.IsEmpty(id))
            {
                ResponseErrorParam(context);
                return Task.CompletedTask;
            }

            var db = Context.GetUserDatabase();
            var user = db.FindUserById(id!);
            if (user == null)
            {
                ResponseNoUser(context, id!);
                return Task.CompletedTask;
            }
            ResponseOk(context, user.ToMap());
            return Task.CompletedTask;
        });
    }
    
    // 验证用户是否还可以进行连接
    private void HandleVerifyUserInfo()
    {
        App.MapGet(Api.ApiVerifyConnectionInfo, context =>
        {
            string? clientId = context.Request.Query[SignalMessage.KeyClientId];
            string? remoteClientId = context.Request.Query[SignalMessage.KeyRemoteClientId];
            string? groupId = context.Request.Query[SignalMessage.KeyGroupId];
            string? pwd = context.Request.Query[SignalMessage.KeyRandomPwd];
            string? maxTunnelCountStr = context.Request.Query[SignalMessage.KeyMaxTunnelCount];
            string? tunnelTypeStr = context.Request.Query[SignalMessage.KeyTunnelType];
            if (Common.IsEmpty(groupId)
                || Common.IsEmpty(clientId)
                || Common.IsEmpty(pwd) 
                || Common.IsEmpty(maxTunnelCountStr) 
                || Common.IsEmpty(tunnelTypeStr)
                || Common.IsEmpty(remoteClientId))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            if (!int.TryParse(maxTunnelCountStr!, out var maxTunnelCount) || maxTunnelCount <= 0)
            {
                ResponseErrorParam(context);
                return Ret();
            }
            if (!int.TryParse(tunnelTypeStr!, out var tunnelType) || tunnelType <= 0)
            {
                ResponseErrorParam(context);
                return Ret();
            }
            
            var db = Context.GetUserDatabase();
            var caller = db.FindUserById(clientId!);
            if (caller == null)
            {
                // todo: 应该是未注册
                ResponseNoUser(context, clientId);
                return Ret();
            }

            var remotePeer = db.FindUserById(remoteClientId!);
            if (remotePeer == null)
            {
                // todo: 应该是未注册
                ResponseNoUser(context, remoteClientId);
                return Ret();
            }
            if (remotePeer.RandomPwd != Common.Md5String(pwd!))
            {
                ResponseRandomPwdError(context);
                return Ret();
            }
            
            if (tunnelType == 1)
            {
                // 一个人发起多个连接算一个
                // 一个人 = 一个通道
                int activeRoomsCount = Context.GetRoomManager().CountActiveRoomsByGroupIdAndClassifyByClientId(groupId!);
                if (activeRoomsCount >= maxTunnelCount)
                {
                    // 已经创建过了，还可以继续创建
                    var rooms = Context.GetRoomManager().GetRoomsCreatedByClientId(clientId!);
                    if (rooms.Count > 0)
                    {
                        ResponseOk(context);
                        return Ret();
                    }
                    
                    // 通道数已达上限，自己从来没创建过，则不能再创建
                    ResponseError(context, Errors.ErrTunnelOverFlow);
                    return Ret();
                }

            } else if (tunnelType == 2)
            {
                // 一个人发起多个连接算多个
                // 一个连接 = 一个通道
                // count active rooms by id
                int activeRoomsCount = Context.GetRoomManager().CountActiveRoomsByGroupId(groupId!);
                if (activeRoomsCount >= maxTunnelCount)
                {
                    ResponseError(context, Errors.ErrTunnelOverFlow);
                    return Ret();
                }
            }
            else
            {
                ResponseError(context, Errors.ErrUnknownTunnelType);
                return Ret();
            }

            ResponseOk(context);
            return Ret();

            Task Ret() => Task.CompletedTask;
        });
    }

    private void HandleSendMessage()
    {
        App.MapPost(Api.ApiSendMessageToClient, context =>
        {
            string? clientId = context.Request.Query[SignalMessage.KeyClientId];
            string? controllerId = context.Request.Query[SignalMessage.KeyControlClientId];
            string? msg = context.Request.Query[SignalMessage.KeyMessage];
            if (Common.IsEmpty(clientId) || Common.IsEmpty(msg) || Common.IsEmpty(controllerId))
            {
                ResponseErrorParam(context);
                return Ret();
            }
            
            Log.Information("client id: " + clientId + " controller id: " + controllerId + " msg : " + msg);
            
            var client = Context.GetClientManager().GetOnlineClientById(clientId!);
            if (client == null)
            {
                ResponseNoUser(context, clientId!);
                return Ret();
            }

            var notifyMsg = SignalMessage.MakeSendMessage(clientId!, controllerId!, msg!);
            client.Notify(notifyMsg);
            
            ResponseOk(context);
            return Ret();
            Task Ret() => Task.CompletedTask;
        });
    }
}