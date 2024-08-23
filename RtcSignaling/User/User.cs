using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RtcSignaling.User;

public class User
{
    [BsonElement("_id")]
    public ObjectId ObjectId = ObjectId.GenerateNewId();
    
    [BsonElement("uid")]
    public string Uid = "";
    
    [BsonElement("clientInfo")]
    public string ClientInfo = "";
    
    [BsonElement("createTimestamp")]
    public long CreateTimestamp;
    
    [BsonElement("lastModifyTimestamp")]
    public long LastModifyTimestamp;
}