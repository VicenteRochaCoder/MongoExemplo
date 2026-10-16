using MongoDB.Bson.Serialization.Attributes;

namespace MongoExemplo.Entities.Base
{
    public class BaseAuditEntity : BaseEntity
    {
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
