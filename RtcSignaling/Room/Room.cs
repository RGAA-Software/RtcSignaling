using Newtonsoft.Json;

namespace RtcSignaling.Room;

public class Room
{
    [JsonProperty("id")]
    public string Id = "";
    
    [JsonProperty("name")]
    public string Name = "";

    [JsonProperty("creator")]
    public string Creator = "";

    [JsonProperty("group_id")]
    public string GroupId = "";
    
    [JsonProperty("clients")]
    private readonly Dictionary<string, Client> _clients = new Dictionary<string, Client>();
    
    private readonly object _clientMutex = new object();
    private bool _alreadyUsed = false;
    
    public void AddClient(Client client)
    {
        lock (_clientMutex)
        {
            if (_clients.ContainsKey(client.Id))
            {
                // todo: warn it
                // client.Close();
                // _clients.Remove(client.Id);
            }
            _clients[client.Id] = client;

            _alreadyUsed = true;
        }
    }

    public void RemoveClient(string clientId)
    {
        lock (_clientMutex)
        {
            if (_clients.ContainsKey(clientId))
            {
                //var client = _clients[clientId];
                //client.Close();
                _clients.Remove(clientId);
            }
        }
    }

    public void VisitClients(Action<Client> callback)
    {
        foreach (var pair in _clients)
        {
            callback(pair.Value);
        }
    }

    public void NotifyAll(string msg)
    {
        lock (_clientMutex)
        {
            VisitClients(client =>
            {
                client.Notify(msg);
            });
        }
    }

    public void NotifyExcept(string exceptId, string msg)
    {
        lock (_clientMutex)
        {
            VisitClients(client =>
            {
                if (client.Id != exceptId)
                {
                    client.Notify(msg);   
                }
            });
        }
    }

    public List<Client> GetClients()
    {
        lock (_clientMutex)
        {
            return [.._clients.Values];
        }
    }

    public List<Client> GetClientsExcept(string exceptId)
    {
        var targetClients = new List<Client>();
        lock (_clientMutex)
        {
            VisitClients(client =>
            {
                if (client.Id != exceptId)
                {
                    targetClients.Add(client);
                }
            });
        }
        return targetClients;
    }

    public bool IsAlreadyUsed()
    {
        return _alreadyUsed;
    }

    public bool Empty()
    {
        lock (_clientMutex)
        {
            return _clients.Count <= 0;
        }
    }

    public bool HasRemotePeer()
    {
        lock (_clientMutex)
        {
            return _clients.Count >= 2;
        }
    }
}