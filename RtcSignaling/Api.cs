namespace RtcSignaling;

public class Api
{
    public const string ApiPing = "/ping";
    // 请求一个9位ID
    public const string ApiRequestClientId = "/request/client/id";
    // 检查客户端是否在线
    public const string ApiCheckClientOnline = "/check/client/online";
    // 同时检查自己和对端是否在线
    public const string ApiCheckClientPairOnline = "/check/client/pair/online";
    // 获取在线的所有客户端，todo:未分页
    public const string ApiOnlineClients = "/online/clients";
    // 获取所有的房间，todo：未分页
    public const string ApiOnlineRooms = "/online/rooms";
    // 获取房间状态
    public const string ApiRequestRoomStatus = "/request/room/status";
    // 总共多少用户
    public const string ApiTotalUsers = "/total/users";
    // 获取用户信息，数据库内保存的信息
    public const string ApiRequestUserInfo = "/request/user/info";
    // 请求一个新的随机密码
    public const string ApiRequestRandomPassword = "/request/random/password";
    // 修改随机密码
    public const string ApiModifyRandomPassword = "/modify/random/password";
    // 修改安全密码
    public const string ApiModifySafetyPassword = "/modify/safety/password";
    // 验证随机密码
    public const string ApiVerifyRandomPassword = "/verify/random/password";
    // 验证连接信息，是否允许连接到对方
    // 1.对方的账号，密码
    // 2.是否在通道数限制之内
    public const string ApiVerifyConnectionInfo = "/verify/connection/info";
    // 按每个连接算一个通道，统计一个group id下的所有工作连接
    // 一个连接 = 一个通道
    public const string ApiRequestActiveRoomsByGroup = "/request/active/rooms/by/group";
    // 按每个连接算一个通道，统计一个group id下的所有工作连接，计算个数
    public const string ApiCountActiveRoomsByGroup = "/count/active/rooms/by/group";
    // 按每个人算一个通道，统计一个group id下的所有工作连接
    // 一个人 = 一个通道
    public const string ApiRequestActiveRoomsByGroupAndClassifyByClientId =
        "/request/active/rooms/by/broup/classify/by/client";
    // 按每个人算一个通道，统计一个group id下的所有工作连接，计算个数
    public const string ApiCountActiveRoomsByGroupAndClassifyByClientId =
        "/count/active/rooms/by/broup/classify/by/client";
    // 获取当前ID创建的房间
    public const string ApiRequestRoomsCreatedByClientId = "/request/rooms/created/by/client";
    // 根据ID向一个客户端发送消息
    public const string ApiSendMessageToClient = "/send/message/to/client";
}