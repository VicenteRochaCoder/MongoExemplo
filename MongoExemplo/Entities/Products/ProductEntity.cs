using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoExemplo.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace MongoExemplo.Entities.Products
{
    /// <summary>
    /// O nome da collection vem do [Table] — o MongoRepository lê esse atributo (igual ao projeto original).
    /// </summary>
    [Table("Products")]
    [BsonIgnoreExtraElements]
    public class ProductEntity : BaseAuditEntity
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("category")]
        public string Category { get; set; }

        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128)]
        public decimal Price { get; set; }

        [BsonElement("stock")]
        public int Stock { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();
    }
}
