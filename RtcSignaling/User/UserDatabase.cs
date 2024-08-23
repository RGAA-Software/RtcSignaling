using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace RtcSignaling.User;

public class UserDatabase
{
    private readonly AppContext _context;
    private MongoClient _mongoClient;
    private IMongoDatabase _mongoDatabase;
    private IMongoCollection<User> _userCollection;

    private const string CollectionUser = "user";
    private const string DatabaseName = "signaling";

    private const string KeyUid = "uid";
    private const string KeyClientInfo = "clientInfo";
    
    public UserDatabase(AppContext ctx)
    {
        _context = ctx;
    }

    public bool Connect(string url)
    {
        try
        {
            _mongoClient = new MongoClient(url);
            _mongoDatabase = _mongoClient.GetDatabase(DatabaseName);
            var findUser = false;
            _mongoDatabase.ListCollectionNames().ForEachAsync((name, idx) =>
            {
                if (name == "user")
                {
                    findUser = true;
                }
            });
            if (!findUser)
            {
                _mongoDatabase.CreateCollection(CollectionUser);
            }
            _userCollection = _mongoDatabase.GetCollection<User>(CollectionUser);
        }
        catch (Exception e)
        {
            Log.Error("Create mongodb client failed : " + e.Message);
            return false;
        }

        return true;
    }

    public User? FindUserById(string id)
    {
        var filter = Builders<User>.Filter;
        var u = _userCollection.Find(filter.Eq(KeyUid, id)).FirstOrDefault(CancellationToken.None);
        return u;
    }

    public User? FindUserByClientInfo(string clientInfo)
    {
        var filter = Builders<User>.Filter.Eq(KeyClientInfo, clientInfo);
        return _userCollection.Find(filter).FirstOrDefault(CancellationToken.None);
    }
    
    public bool SaveUser(User user)
    {
        _userCollection.InsertOne(user);
        return true;
    }

    public long GetTotalUsers()
    {
        return _userCollection.CountDocuments(new BsonDocument());
    }
}