using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoExemplo.Entities.Base
{
    public class BaseEntity
    {
        [BsonId]
        [BsonElement("_id")]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}
