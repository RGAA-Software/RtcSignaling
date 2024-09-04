namespace RtcSignaling.Room;

public class RoomHandler : BaseHttpHandler
{
    public RoomHandler(AppContext ctx, WebApplication app) : base(ctx, app)
    {
        RegisterHandlers();
    }

    protected sealed override void RegisterHandlers()
    {
        HandleRequestActiveRooms();
        HandleCountActiveRooms();
        HandleRequestActiveRoomsAndClassifyByClient();
        HandleCountActiveRoomsAndClassifyByClient();
        HandleRequestRoomsCreatedByClientId();
    }

    private void HandleRequestActiveRooms()
    {
        App.MapGet(Api.ApiRequestActiveRoomsByGroup, context =>
        {
            string? groupId = context.Request.Query[SignalMessage.KeyGroupId];
            if (Common.IsEmpty(groupId))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            var rooms = Context.GetRoomManager().GetActiveRoomsByGroupId(groupId!);
            ResponseOk(context, new Dictionary<string, object>
            {
                {"rooms", rooms}
            });
            
            return Ret();
            Task Ret() => Task.CompletedTask;
        });
    }
    
    private void HandleCountActiveRooms()
    {
        App.MapGet(Api.ApiCountActiveRoomsByGroup, context =>
        {
            string? groupId = context.Request.Query[SignalMessage.KeyGroupId];
            if (Common.IsEmpty(groupId))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            var count = Context.GetRoomManager().CountActiveRoomsByGroupId(groupId!);
            ResponseOk(context, new Dictionary<string, object>
            {
                {"count", count}
            });
            
            return Ret();
            Task Ret() => Task.CompletedTask;
        });
    }

    private void HandleRequestActiveRoomsAndClassifyByClient()
    {
        App.MapGet(Api.ApiRequestActiveRoomsByGroupAndClassifyByClientId, context =>
        {
            string? groupId = context.Request.Query[SignalMessage.KeyGroupId];
            if (Common.IsEmpty(groupId))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            var rooms = Context.GetRoomManager().GetActiveRoomsByGroupIdAndClassifyByClientId(groupId!);
            ResponseOk(context, new Dictionary<string, object>
            {
                {"rooms", rooms}
            });
            
            return Ret();
            Task Ret() => Task.CompletedTask;
        });
    }
    
    private void HandleCountActiveRoomsAndClassifyByClient()
    {
        App.MapGet(Api.ApiCountActiveRoomsByGroupAndClassifyByClientId, context =>
        {
            string? groupId = context.Request.Query[SignalMessage.KeyGroupId];
            if (Common.IsEmpty(groupId))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            var count = Context.GetRoomManager().CountActiveRoomsByGroupIdAndClassifyByClientId(groupId!);
            ResponseOk(context, new Dictionary<string, object>
            {
                {"count", count}
            });
            
            return Ret();
            Task Ret() => Task.CompletedTask;
        });
    }

    private void HandleRequestRoomsCreatedByClientId()
    {
        App.MapGet(Api.ApiRequestRoomsCreatedByClientId, context =>
        {
            string? clientId = context.Request.Query[SignalMessage.KeyClientId];
            if (Common.IsEmpty(clientId))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            var rooms = Context.GetRoomManager().GetRoomsCreatedByClientId(clientId!);
            ResponseOk(context, new Dictionary<string, object>
            {
                {"rooms", rooms}
            });
            
            return Ret();
            Task Ret() => Task.CompletedTask;
        });
    }
}