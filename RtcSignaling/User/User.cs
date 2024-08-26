using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RtcSignaling.User;

public class User
{
    [BsonElement("_id")]
    public ObjectId ObjectId = ObjectId.GenerateNewId();
    
    [BsonElement("uid")]
    public string Uid = "";
    
    [BsonElement("client_info")]
    public string ClientInfo = "";
    
    [BsonElement("create_timestamp")]
    public long CreateTimestamp;
    
    [BsonElement("last_modify_timestamp")]
    public long LastModifyTimestamp;

    [BsonElement("random_pwd")]
    public string RandomPwd = "";
    
    [BsonElement("safety_pwd")]
    public string SafetyPwd = "";


    public Dictionary<string, object> ToMap()
    {
        return new Dictionary<string, object>
        {
            {"obj_id", ObjectId.ToString()},
            {"id", Uid},
            {"client_info", ClientInfo},
            {"create_timestamp", CreateTimestamp},
            {"last_modify_timestamp", LastModifyTimestamp}
        };
    }

}