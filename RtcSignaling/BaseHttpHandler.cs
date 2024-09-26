namespace RtcSignaling;

public class BaseHttpHandler
{
    protected readonly AppContext Context;
    protected readonly WebApplication App;

    protected BaseHttpHandler(AppContext ctx, WebApplication app)
    {
        Context = ctx;
        App = app;
    }

    protected virtual void RegisterHandlers()
    {
        
    }

    public static async void ResponseOk(HttpContext httpContext)
    {
        await httpContext.Response.WriteAsync(Common.MakeOkJsonMessage());
    }

    protected static async void ResponseOk(HttpContext httpContext, Dictionary<string, object> value)
    {
        await httpContext.Response.WriteAsync(Common.MakeOkJsonMessage(value));
    }

    protected static async void ResponseErrorParam(HttpContext httpContext)
    {
        await httpContext.Response.WriteAsync(Errors.MakeKnownErrorMessage(Errors.ErrInvalidParam));
    }

    protected static async void ResponseNoUser(HttpContext httpContext, string id)
    {
        await httpContext.Response.WriteAsync(Errors.MakeKnownErrorMessageExtra(Errors.ErrNoClientFound, id));
    }

    public static void ResponseRandomPwdError(HttpContext httpContext)
    {
        ResponseError(httpContext, Errors.ErrRandomPasswordInvalid);
    }

    public static void ResponseSafetyPwdError(HttpContext httpContext)
    {
        ResponseError(httpContext, Errors.ErrSafetyPasswordInvalid);
    }

    protected static async void ResponseDbError(HttpContext httpContext)
    {
        ResponseError(httpContext, Errors.ErrUpdateDbFailed);
    }
    
    protected static async void ResponseError(HttpContext httpContext, int code)
    {
        await httpContext.Response.WriteAsync(Errors.MakeKnownErrorMessage(code));
    }

}