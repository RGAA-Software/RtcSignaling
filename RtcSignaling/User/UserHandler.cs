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
                ResponseNoUser(context);
                return Task.CompletedTask;
            }
            ResponseOk(context, user.ToMap());
            return Task.CompletedTask;
        });
    }
}