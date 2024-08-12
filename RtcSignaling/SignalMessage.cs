using Newtonsoft.Json;

namespace RtcSignaling;

public class SignalMessage
{
    public const string SigNameHello = "hello";
    public const string SigNameOnHello = "on_hello";
    public const string SigNameCreateRoom = "create_room";
    public const string SigNameOnCreatedRoom = "on_created_room";
    public const string SigNameJoinRoom = "join_room";
    public const string SigNameOnJoinedRoom = "on_joined_room";
    public const string SigNameOnRemoteJoinedRoom = "on_remote_joined_room";
    public const string SigNameLeaveRoom = "leave_room";
    public const string SigNameOnLeftRoom = "on_left_room";
    public const string SigNameOnRemoteLeftRoom = "on_remote_left_room";
    public const string SigNameInviteClient = "invite_client";
    public const string SigNameOnInvitedToRoom = "on_invited_to_room";
    public const string SigNameOnRemoteInvitedToRoom = "on_remote_invited_to_room";
    public const string SigNameHeartBeat = "heart_beat";
    public const string SigNameOnHeartBeat = "on_heart_beat";
    public const string SigNameOfferSdp = "offer_sdp";
    public const string SigNameAnswerSdp = "answer_sdp";
    public const string SigNameIce = "ice";
    public const string SigNameError = "sig_error";
    public const string SigNameForceIFrame = "force_iframe";
    public const string SigNameCommand = "sig_command";
    public const string SigNameOnCommandResponse = "sig_on_command_response";
    public const string SigNameReqControl = "sig_req_control";
    public const string SigNameUnderControl = "sig_under_control";
    public const string SigNameOnDataChannelReady = "on_data_channel_ready";
    public const string SigNameOnRejectControl = "on_reject_control";

    public const string CmdQueryServerStatus = "query_status";

    public const string KeySigName = "sig_name";
    public const string KeyClientId = "client_id";
    public const string KeyRoomId = "room_id";
    public const string KeyRemoteClientId = "remote_client_id";
    public const string KeyControlClientId = "controller_client_id";
    public const string KeySelfClientId = "self_client_id";
    public const string KeyIndex = "index";
    public const string KeyPlatform = "platform";
    public const string KeySdp = "sdp";
    public const string KeyIce = "ice";
    public const string KeyMid = "mid";
    public const string KeySdpMLineIndex = "sdp_m_line_index";
    public const string KeyId = "id";
    public const string KeyName = "name";
    public const string KeyClients = "clients";
    public const string KeyRoom = "room";
    public const string KeyRooms = "rooms";
    public const string KeyAllowReSend = "allow_resend";
    public const string KeyToken = "token";
    
    public class SigBaseMessage
    {
        [JsonProperty("sig_name")]
        public string SigName = "";
        
        [JsonProperty("token")]
        public string Token = "";
        
        [JsonProperty("origin_message")]
        public string OriginMessage = "";
    }
    
    public class SigErrorMessage : SigBaseMessage
    {
        [JsonProperty("sig_code")]
        public int SigCode;
        
        [JsonProperty("sig_info")]
        public string SigInfo = "";
    }

    // SigHelloMessage hello消息
    // client -> server
    public class SigHelloMessage : SigBaseMessage {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("platform")]
        public string Platform = "";
        
        [JsonProperty("allow_resend")]
        public bool AllowReSend = false;
    }
    
    // SigOnHelloMessage hello回复
    // server -> client
    public class SigOnHelloMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
    }
    
    // SigCreateRoomMessage 请求创建一个房间，如果已经存在，则直接返回
    // client -> server
    public class SigCreateRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
    }
    
    // SigOnCreatedRoomMessage 创建完成回调给发起创建者
    // server -> client
    public class SigOnCreatedRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("platform")]
        public string Platform = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }
    
    // SigJoinRoomMessage 加入一个房间
    // client -> server
    public class SigJoinRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
    }
    
    // SigOnJoinedRoomMessage 加入一个房间后，回调给请求加入的人
    // server -> client
    public class SigOnJoinedRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }
    
    // SigOnRemoteJoinedRoomMessage 其他成员加入
    // server -> client
    public class SigOnRemoteJoinedRoomMessage : SigBaseMessage 
    {
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }
    
    // SigLeaveRoomMessage 请求离开房间
    // client -> server
    public class SigLeaveRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
    }
    
    // SigOnLeftRoomMessage 自己离开房间
    // server -> client
    public class SigOnLeftRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }

    // SigOnRemoteLeftRoomMessage 其他成员离开
    // server -> client
    public class SigOnRemoteLeftRoomMessage : SigBaseMessage 
    {
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }

    // SigInviteClientMessage 邀请其他人加入房间
    // client -> server
    public class SigInviteClientMessage : SigBaseMessage  
    {
        [JsonProperty("client_id")]
        public string ClientId = ""; 
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = ""; 
        
        [JsonProperty("room_id")]
        public string RoomId = "";
    }
    
    // SigOnInvitedToRoomMessage 被邀请的人收到这个回调
    // server -> peer client
    public class SigOnInvitedToRoomMessage : SigBaseMessage 
    {
        [JsonProperty("invitor_client_id")]
        public string InvitorClientId = "";
        
        [JsonProperty("self_client_id")]
        public string SelfClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }
    
    // SigOnRemoteInvitedToRoomMessage 发起邀请的人收到这个回调
    // server -> request client
    public class SigOnRemoteInvitedToRoomMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("clients")]
        public List<Client> Clients = [];
    }
    
    // SigHeartBeatMessage 心跳
    // client -> server
    public class SigHeartBeatMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("index")]
        public long Index = 0;
        
        [JsonProperty("platform")]
        public string Platform = "";
    }
    
    // SigOnHeartBeatMessage 心跳回复
    // server -> client
    public class SigOnHeartBeatMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("index")]
        public long Index = 0;
        
        [JsonProperty("platform")]
        public string Platform = "";
    }

    // SigOfferSdpMessage 客户端发过来的Sdp
    // client -> server -> remote client
    public class SigOfferSdpMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("sdp")]
        public string Sdp = "";
    }
    
    // SigAnswerSdpMessage 服务端响应的Sdp
    // remote client -> server -> client
    public class SigAnswerSdpMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("sdp")]
        public string Sdp = "";
    }
    
    // SigIceMessage 两端交互的ICE
    // client <---> remote client
    public class SigIceMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
        
        [JsonProperty("ice")]
        public string Ice = "";
        
        [JsonProperty("mid")]
        public string Mid = "";
        
        [JsonProperty("sdp_m_line_index")]
        public long SdpMLineIndex = 0;
    }

    // SigForceIFrameMessage 产生关键帧
    // client <---> remote client
    public class SigForceIFrameMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
    }
    
    // SigCommandMessage 控制或命令
    public class SigCommandMessage : SigBaseMessage 
    {
        [JsonProperty("command")]
        public string Command = "";
        
        [JsonProperty("extra")]
        public Dictionary<string, string>? Extra = null;
    }
    
    // SigOnCommandResponseMessage 执行结果
    public class SigOnCommandResponseMessage : SigBaseMessage 
    {
        [JsonProperty("command")]
        public string Command = "";
        
        [JsonProperty("info")]
        public Dictionary<string, string>? Info = null;
    }
    
    // SigReqControlMessage client -> server 请求控制
    public class SigReqControlMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("remote_client_id")]
        public string RemoteClientId = "";
    }
    
    // SigUnderControlMessage server -> client 请求控制
    public class SigUnderControlMessage : SigBaseMessage 
    {
        [JsonProperty("self_client_id")]
        public string SelfClientId = "";
        
        [JsonProperty("controller_client_id")]
        public string ControllerId = "";
    }
    
    // SigOnRemoteDataChannelReadyMessage server -> client 数据通道已经建立
    public class SigOnRemoteDataChannelReadyMessage : SigBaseMessage 
    {
        [JsonProperty("self_client_id")]
        public string SelfClientId = "";
        
        [JsonProperty("controller_client_id")]
        public string ControllerId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
    }
    
    // SigOnRejectControlMessage server -> client
    public class SigOnRejectControlMessage : SigBaseMessage 
    {
        [JsonProperty("client_id")]
        public string ClientId = "";
        
        [JsonProperty("controller_client_id")]
        public string ControllerId = "";
        
        [JsonProperty("room_id")]
        public string RoomId = "";
    }

    public static string MakeOnSigKnownErrorMessage(string token, int code)
    {
        return MakeOnSigErrorMessage(token, code, Errors.ErrorString(code));
    }

    public static string MakeOnSigErrorMessage(string token, int code, string info)
    {
        var msg = new SigErrorMessage
        {
            SigName = SigNameError,
            Token = token,
            SigCode = code,
            SigInfo = info
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnHelloMessage(string token, string clientId)
    {
        var msg = new SigOnHelloMessage
        {
            SigName = SigNameOnHello,
            Token = token,
            ClientId = clientId,
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnCreatedRoomMessage(string token, string clientId, string remoteClientId, Room room)
    {
        var msg = new SigOnCreatedRoomMessage
        {
            SigName = SigNameOnCreatedRoom,
            Token = token,
            ClientId = clientId,
            RemoteClientId = remoteClientId,
            RoomId = room.Id,
            Clients = room.GetClients(),
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnHeartBeatMessage(SigHeartBeatMessage hbMsg)
    {
        var msg = new SigOnHeartBeatMessage
        {
            SigName = SigNameOnHeartBeat,
            Token = hbMsg.Token,
            ClientId = hbMsg.ClientId,
            Index = hbMsg.Index
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnJoinedRoomMessage(string token, Room room, string clientId, string remoteClientId)
    {
        var msg = new SigOnJoinedRoomMessage
        {
            SigName = SigNameOnJoinedRoom,
            Token = token,
            RoomId = room.Id,
            ClientId = clientId,
            RemoteClientId = remoteClientId,
            Clients = room.GetClients(),
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnRemoteJoinedRoomMessage(string token, Room room, string remoteClientId)
    {
        var msg = new SigOnRemoteJoinedRoomMessage
        {
            SigName = SigNameOnRemoteJoinedRoom,
            Token = token,
            RoomId = room.Id,
            RemoteClientId = remoteClientId,
            Clients = room.GetClients(),
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnLeftRoomMessage(string token, Room room, string clientId)
    {
        var msg = new SigOnLeftRoomMessage
        {
            SigName = SigNameOnLeftRoom,
            Token = token,
            ClientId = clientId,
            RoomId = room.Id,
            Clients = room.GetClients(),
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnRemoteClientLeftMessage(string token, Room room, string leftClientId)
    {
        var msg = new SigOnRemoteLeftRoomMessage
        {
            SigName = SigNameOnRemoteLeftRoom,
            Token = token,
            RemoteClientId = leftClientId,
            RoomId = room.Id,
            Clients = room.GetClients()
        };
        return JsonConvert.SerializeObject(msg);
    }
    
    // MakeOnInvitedToRoomMessage 通知被邀请者，已经加入Room了
    public static string MakeOnInvitedToRoomMessage(string token, Room room, string invitorClientId, string selfId)
    {
        var msg = new SigOnInvitedToRoomMessage
        {
            SigName = SigNameOnInvitedToRoom,
            Token = token,
            InvitorClientId = invitorClientId,
            SelfClientId = selfId,
            RoomId = room.Id,
            Clients = room.GetClients(),
        };
        return JsonConvert.SerializeObject(msg);
    }
    
    // MakeOnRemoteInvitedToRoomMessage 通知到发起邀请者,对方已经被邀请进Room了
    public static string MakeOnRemoteInvitedToRoomMessage(string token, Room room, string clientId, string remoteClientId)
    {
        var msg = new SigOnRemoteInvitedToRoomMessage
        {
            SigName = SigNameOnRemoteInvitedToRoom,
            Token = token,
            ClientId = clientId,
            RemoteClientId = remoteClientId,
            RoomId = room.Id,
            Clients = room.GetClients(),
        };
        return JsonConvert.SerializeObject(msg);
    }

    public static string MakeOnCommandResponseMessage(SigOnCommandResponseMessage msg)
    {
        return JsonConvert.SerializeObject(msg);
    }
}