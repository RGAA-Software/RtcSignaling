using System.Text.Json;
using System.Text.Json.Nodes;
using Serilog;
using Newtonsoft.Json;
using RtcSignaling.Room;

namespace RtcSignaling;

public class SignalProcessor
{
    private readonly AppContext _context;
    private readonly ClientManager _clientManager;
    private readonly RoomManager _roomManager;
    private readonly WebSocketHandler _handler;
    private readonly Client _client;
    
    public SignalProcessor(AppContext ctx, WebSocketHandler handler)
    {
        _context = ctx;
        _clientManager = ctx.GetClientManager();
        _roomManager = ctx.GetRoomManager();
        _handler = handler;
        _client = new Client();
        _client.SetWebSocket(handler);
    }

    private static string GetStringValue(Dictionary<string, object> obj, string key)
    {
        var v = obj[key].ToString();
        return v ?? "";
    }

    private static long GetInt64Value(Dictionary<string, object> obj, string key)
    {
        var v = Convert.ToInt64(obj[key]);
        return v;
    }

    public bool ParseMessage(string message, WebSocketHandler wsHandler)
    {
        try
        {
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
            if (jsonObject == null)
            {
                return false;
            }
            
            var sigName = jsonObject[SignalMessage.KeySigName].ToString();
            var token = "";
            if (jsonObject.TryGetValue(SignalMessage.KeyToken, out var value))
            {
                token = value.ToString();   
            }
            if (token == null || token.Length <= 0)
            {
                if (sigName != SignalMessage.SigNameHeartBeat && sigName != SignalMessage.SigNameHello)
                {
                    var msg = "Token is empty, msg: " + message;
                    Log.Error(msg);
                    Console.WriteLine(msg);
                    return false;
                }
            }

            var groupId = "";
            if (jsonObject.TryGetValue(SignalMessage.KeyGroupId, out var gid))
            {
                groupId = gid.ToString();
            }

            var userId = "";
            if (jsonObject.TryGetValue(SignalMessage.KeyUserId, out var uid))
            {
                userId = uid.ToString();
            }
            
            if (sigName == SignalMessage.SigNameHello)
            {
                // hello
                var allowReSend = false;
                if (jsonObject.TryGetValue(SignalMessage.KeyAllowReSend, out var value1))
                {
                    allowReSend = Convert.ToBoolean(value1);
                }
                _onSigHelloCbk(new SignalMessage.SigHelloMessage
                {
                    SigName = sigName,
                    Token = token,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    Platform = GetStringValue(jsonObject, SignalMessage.KeyPlatform),
                    AllowReSend = allowReSend,
                    GroupId = groupId,
                    UserId = userId,
                }, wsHandler);
                
            } else if (sigName == SignalMessage.SigNameCreateRoom)
            {
                // create room
                _onSigCreateRoomCbk(new SignalMessage.SigCreateRoomMessage
                {
                    SigName = sigName,
                    Token = token,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RemoteClientId = GetStringValue(jsonObject, SignalMessage.KeyRemoteClientId),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameJoinRoom)
            {
                // join room
                _onSigJoinRoomCbk(new SignalMessage.SigJoinRoomMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RemoteClientId = GetStringValue(jsonObject, SignalMessage.KeyRemoteClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameInviteClient)
            {
                OnSigInviteClientCbk(new SignalMessage.SigInviteClientMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RemoteClientId = GetStringValue(jsonObject, SignalMessage.KeyRemoteClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    GroupId = groupId,
                    UserId = userId,
                });
            } 
            else if (sigName == SignalMessage.SigNameLeaveRoom)
            {
                // leave room
                _onSigLeaveRoomCbk(new SignalMessage.SigLeaveRoomMessage
                {
                    SigName = sigName,
                    Token = token,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameHeartBeat)
            {
                // heart beat
                _onSigHeartBeatCbk(new SignalMessage.SigHeartBeatMessage
                {
                    SigName = sigName,
                    Token = token,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    Index = GetInt64Value(jsonObject, SignalMessage.KeyIndex),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameOfferSdp)
            {
                // offer sdp
                _onSigOfferSdpCbk(new SignalMessage.SigOfferSdpMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    Sdp = GetStringValue(jsonObject, SignalMessage.KeySdp),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameAnswerSdp)
            {
                // answer sdp
                _onSigAnswerSdpCbk(new SignalMessage.SigAnswerSdpMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    Sdp = GetStringValue(jsonObject, SignalMessage.KeySdp),
                    GroupId = groupId,
                    UserId = userId,
                });

            } else if (sigName == SignalMessage.SigNameIce)
            {
                // ice
                _onSigIceCbk(new SignalMessage.SigIceMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    Ice = GetStringValue(jsonObject, SignalMessage.KeyIce),
                    Mid = GetStringValue(jsonObject, SignalMessage.KeyMid),
                    SdpMLineIndex = GetInt64Value(jsonObject, SignalMessage.KeySdpMLineIndex),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameForceIFrame)
            {
                // force i frame
                _onSigForceIFrameCbk(new SignalMessage.SigForceIFrameMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameReqControl)
            {
                // request control
                _onSigReqControlCbk(new SignalMessage.SigReqControlMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RemoteClientId = GetStringValue(jsonObject, SignalMessage.KeyRemoteClientId),
                    GroupId = groupId,
                    UserId = userId,
                });

            } else if (sigName == SignalMessage.SigNameUnderControl)
            {
                // under control
                _onSigUnderControlCbk(new SignalMessage.SigUnderControlMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    SelfClientId = GetStringValue(jsonObject, SignalMessage.KeySelfClientId),
                    ControllerId = GetStringValue(jsonObject, SignalMessage.KeyControlClientId),
                    GroupId = groupId,
                    UserId = userId,
                });
                
            } else if (sigName == SignalMessage.SigNameOnDataChannelReady)
            {
                // data channel ready
                _onSigRemoteDataChannelReadyCbk(new SignalMessage.SigOnRemoteDataChannelReadyMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    SelfClientId = GetStringValue(jsonObject, SignalMessage.KeySelfClientId),
                    ControllerId = GetStringValue(jsonObject, SignalMessage.KeyControlClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    GroupId = groupId,
                    UserId = userId,
                });

            } else if (sigName == SignalMessage.SigNameOnRejectControl)
            {
                // reject control
                _onRejectControlCbk(new SignalMessage.SigOnRejectControlMessage
                {
                    SigName = sigName,
                    Token = token,
                    OriginMessage = message,
                    ClientId = GetStringValue(jsonObject, SignalMessage.KeyClientId),
                    RoomId = GetStringValue(jsonObject, SignalMessage.KeyRoomId),
                    ControllerId = GetStringValue(jsonObject, SignalMessage.KeyControlClientId),
                    GroupId = groupId,
                    UserId = userId,
                });
            }
            return true;
        }
        catch (Exception e)
        {
            Log.Error("Parse json failed: " + e.StackTrace + ", msg: " + e.Message);
            Console.WriteLine("Parse json failed: " + e.StackTrace + ", msg: " + message);
            return false;
        }
    }

    private Room.Room? GetRoomById(string roomId, Client reqClient)
    {
        var room = _context.GetRoomManager().FindRoomById(roomId);
        if (room == null)
        {
            reqClient.Notify(Errors.MakeKnownErrorMessage(Errors.ErrNoRoomFound));
            Log.Error("Can't find room: " + roomId + ", req client: " + reqClient.Id);
        }
        return room;
    }

    private static bool IsClientIdOk(string clientId, string fromMethod)
    {
        if (clientId.Length == 9)
        {
            return true;
        }
        Log.Error("Error client id: " + clientId + ", from method: " + fromMethod);
        return false;
    }

    private void _onSigHelloCbk(SignalMessage.SigHelloMessage msg, WebSocketHandler wsHandler)
    {
        if (!IsClientIdOk(msg.ClientId, "Hello")) return;
        
        if (_clientManager.IsClientOnline(msg.ClientId) && !msg.AllowReSend)
        {
            Log.Error("You need to AllowResend.");
            _handler.SendMessage(Errors.MakeKnownErrorMessage(Errors.ErrAlreadyLogin));
            return;
        }

        _client.Id = msg.ClientId;
        _client.Token = msg.Token;
        _client.Platform = msg.Platform;
        _client.UpdateTimestamp = Common.GetCurrentTimestamp();
        _client.GroupId = msg.GroupId;
        _client.UserId = msg.UserId;
        _clientManager.AddClient(_client);
        _client.Notify(SignalMessage.MakeOnHelloMessage(_client.Token, _client.Id));
        
        wsHandler.AssociateWith(_client);
        Log.Information("OnHello: " + _client.ToString());
    }

    private void _onSigCreateRoomCbk(SignalMessage.SigCreateRoomMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "CreateRoom")) return;
        var room = _roomManager.CreateRoom(msg.Token, msg.ClientId, msg.RemoteClientId, msg.GroupId);
        _client.Id = msg.ClientId;
        _client.Token = msg.Token;
        _client.Notify(SignalMessage.MakeOnCreatedRoomMessage(msg.Token, msg.ClientId, msg.RemoteClientId, room));
        Log.Information("CreateRoom, token: " + msg.Token + ", client id: " + msg.ClientId 
                        + ", remote client id: " + msg.RemoteClientId + ", group id: " + msg.GroupId);
    }
    
    private void _onSigJoinRoomCbk(SignalMessage.SigJoinRoomMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "JoinRoom")) return;
        
        // find room
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        
        // enter room
        room.AddClient(_client);
        
        // telling the client, you are already in room now. 
        _client.Notify(SignalMessage.MakeOnJoinedRoomMessage(msg.Token, room, msg.ClientId, msg.RemoteClientId));
        // telling the other client, new client in.
        room.NotifyExcept(msg.ClientId, SignalMessage.MakeOnRemoteJoinedRoomMessage(msg.Token, room, msg.RemoteClientId));
    }
    
    private void OnSigInviteClientCbk(SignalMessage.SigInviteClientMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "InviteClient")) return;
        if (!IsClientIdOk(msg.RemoteClientId, "InviteClient")) return;
        
        // remote client
        var remoteClient = _clientManager.GetOnlineClientById(msg.RemoteClientId);
        if (remoteClient == null)
        {
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrClientOffline));
            Log.Error("Can't find remote client: " + msg.RemoteClientId);
            return;
        }
        
        // room
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        
        room.AddClient(remoteClient);
        // telling the remote peer, you are invited to room
        remoteClient.Notify(SignalMessage.MakeOnInvitedToRoomMessage(msg.Token, room, msg.ClientId, msg.RemoteClientId));
        // telling the invitor, remote peer is in room now
        _client.Notify(SignalMessage.MakeOnRemoteInvitedToRoomMessage(msg.Token, room, msg.ClientId, msg.RemoteClientId));
    }
    
    private void _onSigLeaveRoomCbk(SignalMessage.SigLeaveRoomMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "LeaveRoom")) return;
        
        // find room
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        
        // remove the client 
        room.RemoveClient(msg.ClientId);
        // telling the client, you left
        _client.Notify(SignalMessage.MakeOnLeftRoomMessage(msg.Token, room, msg.ClientId));
        // telling the other client, someone left
        room.NotifyExcept(msg.ClientId, SignalMessage.MakeOnRemoteClientLeftMessage(msg.Token, room, msg.ClientId));
    }

    private void _onSigHeartBeatCbk(SignalMessage.SigHeartBeatMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "HeartBeat")) return;

        var client = _clientManager.GetOnlineClientById(msg.ClientId);
        if (client == null)
        {
            _client.Id = msg.ClientId;
            _client.Token = msg.Token;
            _client.Platform = msg.Platform;
            _client.UpdateTimestamp = Common.GetCurrentTimestamp();
            _clientManager.AddClient(_client);
        }
        _client.OnHeartBeat(msg);
    }
    
    private void _onSigOfferSdpCbk(SignalMessage.SigOfferSdpMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "OfferSdp")) return;
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        room.NotifyExcept(msg.ClientId, msg.OriginMessage);
    }
    
    private void _onSigAnswerSdpCbk(SignalMessage.SigAnswerSdpMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "AnswerSdp")) return;
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        room.NotifyExcept(msg.ClientId, msg.OriginMessage);
    }
    
    private void _onSigIceCbk(SignalMessage.SigIceMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "Ice")) return;
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        room.NotifyExcept(msg.ClientId, msg.OriginMessage);
    }
    
    private void _onSigForceIFrameCbk(SignalMessage.SigForceIFrameMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "IFrame")) return;
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        room.NotifyExcept(msg.ClientId, msg.OriginMessage);   
    }
    
    private void _onSigReqControlCbk(SignalMessage.SigReqControlMessage msg)
    {
        if (!IsClientIdOk(msg.ClientId, "ReqControl")) return;
        if (!IsClientIdOk(msg.RemoteClientId, "ReqControl")) return;
        var remoteClient = _clientManager.GetOnlineClientById(msg.RemoteClientId);
        if (remoteClient == null)
        {
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrClientOffline));
            Log.Error("[ReqControl]Can't find remote client: " + msg.RemoteClientId);
            return;
        }
        remoteClient.Notify(msg.OriginMessage);
    }

    private void _onSigUnderControlCbk(SignalMessage.SigUnderControlMessage msg)
    {
        if (!IsClientIdOk(msg.ControllerId, "UnderControl")) return;
        var controllerClient = _clientManager.GetOnlineClientById(msg.ControllerId);
        if (controllerClient == null)
        {
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrClientOffline));
            Log.Error("[UnderControl]Can't find remote client: " + msg.ControllerId);
            return;
        }
        controllerClient.Notify(msg.OriginMessage);
    }
    
    private void _onSigRemoteDataChannelReadyCbk(SignalMessage.SigOnRemoteDataChannelReadyMessage msg)
    {
        if (!IsClientIdOk(msg.ControllerId, "DataChannelReady")) return;
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        room.NotifyExcept(msg.SelfClientId, msg.OriginMessage);
    }

    private void _onRejectControlCbk(SignalMessage.SigOnRejectControlMessage msg)
    {
        if (!IsClientIdOk(msg.ControllerId, "RejectControl")) return;
        var room = _roomManager.FindRoomById(msg.RoomId);
        if (room == null)
        {
            Log.Error("Can't find room: " + msg.RoomId);
            _client.Notify(SignalMessage.MakeOnSigKnownErrorMessage(msg.Token, Errors.ErrNoRoomFound));
            return;
        }
        room.NotifyExcept(msg.ClientId, msg.OriginMessage);
    }
}