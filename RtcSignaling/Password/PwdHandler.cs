namespace RtcSignaling.Password;

public class PwdHandler : BaseHttpHandler
{
    public PwdHandler(AppContext ctx, WebApplication app) : base(ctx, app)
    {
        RegisterHandlers();
    }
    
    protected sealed override void RegisterHandlers()
    {
        HandleRequestRandomPwd();
        HandleModifySafetyPwd();
        HandleVerifyRandomPassword();
        HandleModifyRandomPassword();
    }

    private void HandleRequestRandomPwd()
    {
        App.MapGet(Api.ApiRequestRandomPassword, context =>
        {
            // check id
            string? id = context.Request.Query[SignalMessage.KeyClientId];
            string? oldRandomPwd = context.Request.Query[SignalMessage.KeyOldRandomPwd];
            if (Common.IsEmpty(id) /*|| Common.IsEmpty(oldRandomPwd)*/)
            {
                ResponseErrorParam(context);
                return Task.CompletedTask;
            }

            var db = Context.GetUserDatabase();
            // check user 
            var user = db.FindUserById(id);
            if (user == null)
            {
                ResponseNoUser(context, id);
                return Task.CompletedTask;
            }
            
            // check old random password
            // if (user.RandomPwd != Common.Md5String(oldRandomPwd))
            // {
            //     ResponseRandomPwdError(context);
            //     return Task.CompletedTask;
            // }

            // generate random password
            var randomPwd = RandomPwdGenerator.GenRandomPassword();
            if (db.UpdateRandomPwd(id, Common.Md5String(randomPwd)))
            {
                ResponseOk(context, new Dictionary<string, object>
                {
                    {SignalMessage.KeyId, id},
                    {SignalMessage.KeyRandomPwd, randomPwd}
                });
            }
            else
            {
                ResponseError(context, Errors.ErrUpdateDbFailed);
            }

            return Task.CompletedTask;
        });
    }

    private void HandleModifySafetyPwd()
    {
        App.MapPost(Api.ApiModifySafetyPassword, context =>
        {
            // check id
            string? id = context.Request.Query[SignalMessage.KeyClientId];
            string? oldSafetyPwd = context.Request.Query[SignalMessage.KeyOldSafetyPwd];
            string? newSafetyPwd = context.Request.Query[SignalMessage.KeySafetyPwd];
            if (Common.IsEmpty(id) || Common.IsEmpty(newSafetyPwd))
            {
                ResponseErrorParam(context);
                return Task.CompletedTask;
            }

            var db = Context.GetUserDatabase();
            // check user 
            var user = db.FindUserById(id);
            if (user == null)
            {
                ResponseNoUser(context, id);
                return Task.CompletedTask;
            }
            
            // check old safety password
            if (!Common.IsEmpty(user.SafetyPwd) && user.SafetyPwd != Common.Md5String(oldSafetyPwd))
            {
                ResponseSafetyPwdError(context);
                return Task.CompletedTask;
            }
            
            // update
            if (db.UpdateSafetyPwd(id, Common.Md5String(newSafetyPwd)))
            {
                ResponseOk(context, new Dictionary<string, object>
                {
                    {SignalMessage.KeyId, id},
                    {SignalMessage.KeySafetyPwd, newSafetyPwd}
                });
            }
            else
            {
                ResponseError(context, Errors.ErrUpdateDbFailed);
            }

            return Task.CompletedTask;
        });
    }

    private void HandleVerifyRandomPassword()
    {
        App.MapGet(Api.ApiVerifyRandomPassword, context =>
        {
            string? clientId = context.Request.Query[SignalMessage.KeyClientId];
            string? pwd = context.Request.Query[SignalMessage.KeyRandomPwd];
            if (Common.IsEmpty(clientId) || Common.IsEmpty(pwd))
            {
                ResponseErrorParam(context);
                return Ret();
            }

            var db = Context.GetUserDatabase();
            var user = db.FindUserById(clientId!);
            if (user == null)
            {
                ResponseNoUser(context, clientId);
                return Ret();
            }
            if (user.RandomPwd != Common.Md5String(pwd!))
            {
                ResponseRandomPwdError(context);
                return Ret();
            }
            
            ResponseOk(context);
            return Ret();

            Task Ret() => Task.CompletedTask;
        });
    }

    private void HandleModifyRandomPassword()
    {
        App.MapGet(Api.ApiModifyRandomPassword, context =>
        {
            string? clientId = context.Request.Query[SignalMessage.KeyClientId];
            string? newPwd = context.Request.Query[SignalMessage.KeyRandomPwd];
            if (Common.IsEmpty(clientId) || Common.IsEmpty(newPwd) || newPwd!.Length < 8)
            {
                ResponseErrorParam(context);
                return Ret();
            }
            
            var db = Context.GetUserDatabase();
            var user = db.FindUserById(clientId!);
            if (user == null)
            {
                ResponseNoUser(context, clientId);
                return Ret();
            }
            
            if (db.UpdateRandomPwd(clientId!, Common.Md5String(newPwd!)))
            {
                ResponseOk(context, new Dictionary<string, object>
                {
                    {SignalMessage.KeyId, clientId!},
                    {SignalMessage.KeyRandomPwd, newPwd!}
                });
            }
            else
            {
                ResponseError(context, Errors.ErrUpdateDbFailed);
            }
            return Ret();
            
            Task Ret() => Task.CompletedTask;
        });
    }
}