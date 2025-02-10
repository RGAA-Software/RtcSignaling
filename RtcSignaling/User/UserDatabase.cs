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

    private const string CollectionUser = "tc_user";
    private const string DatabaseName = "tc_signaling";

    private const string KeyUid = "uid";
    private const string KeyClientInfo = "client_info";
    private const string KeyRandomPwd = "random_pwd";
    private const string KeySafetyPwd = "safety_pwd";
    
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

    public bool UpdateUserById(string id, Dictionary<string, object> values)
    {
        var update = Builders<User>.Update;
        UpdateDefinition<User>? updateDef = null;
        foreach (var pair in values)
        {
            updateDef = update.Set(pair.Key, pair.Value);
        }
        if (updateDef == null)
        {
            Log.Error("no value to update.");
            return false;
        }
        var result = _userCollection.UpdateOne(Builders<User>.Filter.Eq(KeyUid, id), updateDef);
        return result.MatchedCount >= 1;
    }

    public bool UpdateRandomPwd(string id, string randomPwd)
    {
        return UpdateUserById(id, new Dictionary<string, object>
        {
            {KeyRandomPwd, randomPwd},
        });
    }

    public bool UpdateSafetyPwd(string id, string safetyPwd)
    {
        return UpdateUserById(id, new Dictionary<string, object>
        {
            {KeySafetyPwd, safetyPwd},
        });
    }
    
    public long GetTotalUsers()
    {
        return _userCollection.CountDocuments(new BsonDocument());
    }
}