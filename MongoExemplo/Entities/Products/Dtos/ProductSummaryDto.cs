namespace MongoExemplo.Entities.Products.Dtos
{
    /// <summary>
    /// Usado nas projeções: o Mongo devolve só os campos necessários.
    /// </summary>
    public class ProductSummaryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
