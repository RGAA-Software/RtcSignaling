using System.Net.WebSockets;
using Newtonsoft.Json;
using Serilog;

namespace RtcSignaling;

public class Client
{
    [JsonProperty("id")]
    public string Id = "";
    
    [JsonProperty("token")]
    public string Token = "";
    
    [JsonProperty("name")]
    public string Name = "";
    
    [JsonProperty("role")]
    public string Role = "";
    
    [JsonProperty("platform")]
    public string Platform = "";
    
    [JsonProperty("room_id")]
    public string RoomId = "";
    
    [JsonProperty("update_timestamp")]
    public long UpdateTimestamp = 0;
    private WebSocketHandler? _wsHandler = null;

    public void SetWebSocket(WebSocketHandler? socket)
    {
        _wsHandler = socket;
    }
    
    public void Notify(string msg)
    {
        if (_wsHandler == null)
        {
            Log.Error("Empty ws handler.");
            return;
        }
        //Log.Information("Send back: " + msg);
        _wsHandler.SendMessage(msg);
    }

    public void OnHeartBeat(SignalMessage.SigHeartBeatMessage msg)
    {
        UpdateTimestamp = Common.GetCurrentTimestamp();
        if (Id != msg.ClientId && msg.ClientId.Length > 0)
        {
            Id = msg.ClientId;
        }

        var backMsg = SignalMessage.MakeOnHeartBeatMessage(msg);
        //Console.WriteLine("OnHeartBeat: " + backMsg);
        SendMessage(backMsg);
    }

    public bool IsOnline()
    {
        var diff = Common.GetCurrentTimestamp() - UpdateTimestamp;
        return diff < 10000;
    }

    public void SendMessage(string msg)
    {
        Notify(msg);
    }

    public void Close()
    {
        _wsHandler?.Close();
    }
}