namespace RtcSignaling;

public class Errors
{
    public const int NoError = 1000;
    public const int ErrAuth = 600;
    public const int ErrInvalidParam = 601;
    public const int ErrNoRoomFound = 602;
    public const int ErrCreateRoomFailed = 603;
    public const int ErrClientOffline = 604;
    public const int ErrNoClientFound = 605;
    public const int ErrCommandNotProcessed = 606;
    public const int ErrAlreadyLogin = 607;
    public const int ErrUpdateDbFailed = 608;
    public const int ErrRandomPasswordInvalid = 609;
    public const int ErrSafetyPasswordInvalid = 610;
    public const int ErrUnknownTunnelType = 611;
    public const int ErrTunnelOverFlow = 612;
    
    public static string ErrorStringExtra(int err, string extra)
    {
        string errStr = "";
        switch (err)
        {
            case ErrAuth:
                errStr = "鉴权失败";
                break;
            case ErrInvalidParam:
                errStr = "不合法的参数";
                break;
            case ErrNoRoomFound:
                errStr = "没找到此房间";
                break;
            case ErrCreateRoomFailed:
                errStr = "创建房间失败";
                break;
            case ErrClientOffline:
                errStr = "客户端不在线";
                break;
            case ErrNoClientFound:
                errStr = "没有找到客户端";
                break;
            case ErrCommandNotProcessed:
                errStr = "Command处理失败";
                break;
            case ErrAlreadyLogin:
                errStr = "此客户端ID已在线";
                break;
            case ErrUpdateDbFailed:
                errStr = "更新数据库失败";
                break;
            case ErrRandomPasswordInvalid:
                errStr = "密码校验失败";
                break;
            case ErrSafetyPasswordInvalid:
                errStr = "安全密码校验失败";
                break;
            case ErrUnknownTunnelType:
                errStr = "未知的通道类型";
                break;
            case ErrTunnelOverFlow:
                errStr = "通道数溢出，请检查授权通道数";
                break;
            default:
                errStr = "未知错误";
                break;
        }
        return errStr + " " + extra;
    }

    public static string ErrorString(int err)
    {
        return ErrorStringExtra(err, "");
    }

    public static string MakeKnownErrorMessage(int code)
    {
        return Common.MakeJsonMessage(code, ErrorString(code), new Dictionary<string, object>());
    }

    public static string MakeKnownErrorMessageExtra(int code, string extra)
    {
        return Common.MakeJsonMessage(code, ErrorStringExtra(code, extra), new Dictionary<string, object>());
    }
}