using System.Timers;
using Serilog;
using Timer = System.Timers.Timer;

namespace RtcSignaling;

public class ClientManager
{
    private AppContext _context;
    private readonly object _clientsMutex = new();
    private readonly Dictionary<string, Client> _clients = new();
    
    public ClientManager(AppContext ctx)
    {
        _context = ctx;
    }
    
    public void AddClient(Client client)
    {
        lock (_clientsMutex)
        {
            _clients[client.Id] = client;
        }
    }

    public void RemoveClient(string clientId)
    {
        lock (_clientsMutex)
        {
            if (_clients.ContainsKey(clientId))
            {
                _clients.Remove(clientId);
            }
        }
    }

    public bool IsClientOnline(string clientId)
    {
        return GetOnlineClientById(clientId) != null;
    }

    public Client? GetOnlineClientById(string clientId)
    {
        lock (_clientsMutex)
        {
            if (!_clients.TryGetValue(clientId, out var client))
            {
                return null;
            }
            return client.IsOnline() ? client : null;
        }
    }
    
    public void CleanOfflineClients()
    {
        lock (_clientsMutex)
        {
            var toRemoveIds = new List<string>();
            foreach (var pair in _clients)
            {
                if (!pair.Value.IsOnline())
                {
                    toRemoveIds.Add(pair.Key);
                }
            }
            foreach (var removeId in toRemoveIds)
            {
                _clients.Remove(removeId);
                Log.Information("Clean offline client: " + removeId);
            }
        }
    }

    public List<Client> GetOnlineClients()
    {
        lock (_clientsMutex)
        {
            var onlineClients = new List<Client>();
            foreach (var pair in _clients)
            {
                if (pair.Value.IsOnline())
                {
                    onlineClients.Add(pair.Value);
                }
            }
            return onlineClients;
        }
    }
}