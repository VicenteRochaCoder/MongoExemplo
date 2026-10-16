using MongoDB.Bson;
using MongoDB.Driver;
using MongoExemplo.Entities.Products.Dtos;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace MongoExemplo.Entities.Products
{
    /// <summary>
    /// Specification = lugar único onde ficam os filtros, ordenações e projeções de uma entidade.
    /// Os handlers não montam Builders na mão: eles combinam essas peças e mandam para o IMongoRepository.
    /// </summary>
    public static class ProductEntitySpecification
    {
        private static FilterDefinitionBuilder<ProductEntity> Filter => Builders<ProductEntity>.Filter;

        #region Filtros simples
        public static FilterDefinition<ProductEntity> ById(string id) => Filter.Eq(e => e.Id, id);

        public static FilterDefinition<ProductEntity> Active() => Filter.Eq(e => e.IsActive, true);

        public static FilterDefinition<ProductEntity> ByCategory(string category) => Filter.Eq(e => e.Category, category);

        /// <summary>Busca "contém", sem diferenciar maiúsculas/minúsculas.</summary>
        public static FilterDefinition<ProductEntity> ByNameContains(string term) =>
            Filter.Regex(e => e.Name, new BsonRegularExpression(Regex.Escape(term), "i"));

        public static FilterDefinition<ProductEntity> ByPriceRange(decimal? min, decimal? max)
        {
            var filters = new List<FilterDefinition<ProductEntity>>();
            if (min.HasValue) filters.Add(Filter.Gte(e => e.Price, min.Value));
            if (max.HasValue) filters.Add(Filter.Lte(e => e.Price, max.Value));
            return filters.Count == 0 ? Filter.Empty : Filter.And(filters);
        }

        public static FilterDefinition<ProductEntity> WithTag(string tag) => Filter.AnyEq(e => e.Tags, tag);

        public static FilterDefinition<ProductEntity> OutOfStock() => Filter.Lte(e => e.Stock, 0);
        #endregion

        #region Filtro composto
        /// <summary>
        /// Monta o filtro a partir de parâmetros opcionais (ex.: filtros da tela/grid).
        /// Só entra no filtro o que veio preenchido; sempre traz apenas os ativos.
        /// </summary>
        public static FilterDefinition<ProductEntity> BySearch(string name = null, string category = null,
            decimal? minPrice = null, decimal? maxPrice = null, string tag = null)
        {
            var filters = new List<FilterDefinition<ProductEntity>> { Active() };

            if (!string.IsNullOrWhiteSpace(name)) filters.Add(ByNameContains(name));
            if (!string.IsNullOrWhiteSpace(category)) filters.Add(ByCategory(category));
            if (minPrice.HasValue || maxPrice.HasValue) filters.Add(ByPriceRange(minPrice, maxPrice));
            if (!string.IsNullOrWhiteSpace(tag)) filters.Add(WithTag(tag));

            return Filter.And(filters);
        }
        #endregion

        #region Ordenação
        public static SortDefinition<ProductEntity> OrderByPriceAsc() => Builders<ProductEntity>.Sort.Ascending(e => e.Price);

        public static SortDefinition<ProductEntity> OrderByNewest() => Builders<ProductEntity>.Sort.Descending(e => e.CreatedDate);
        #endregion

        #region Projeção
        public static ProjectionDefinition<ProductEntity, ProductSummaryDto> SummaryProjection() =>
            BuildProjection(e => new ProductSummaryDto { Id = e.Id, Name = e.Name, Price = e.Price });

        /// <summary>Mesmo helper genérico usado no TicketEntitySpecification do projeto original.</summary>
        public static ProjectionDefinition<ProductEntity, TProjection> BuildProjection<TProjection>(
            Expression<Func<ProductEntity, TProjection>> projectionExpression) =>
            Builders<ProductEntity>.Projection.Expression(projectionExpression);
        #endregion

        #region Updates
        public static UpdateDefinition<ProductEntity> Deactivate() => Builders<ProductEntity>.Update.Set(e => e.IsActive, false);

        public static UpdateDefinition<ProductEntity> AddStock(int quantity) => Builders<ProductEntity>.Update.Inc(e => e.Stock, quantity);
        #endregion
    }
}
