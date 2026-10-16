using MongoDB.Bson;
using MongoDB.Driver;
using MongoExemplo.Entities.Base;
using MongoExemplo.Services;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;

namespace MongoExemplo.Repositories.Impl
{
    public class MongoRepository(IMongoClient client, ITenantSelectionService tenantSelectionService) : IMongoRepository
    {
        private static readonly ConcurrentDictionary<Type, string> _collectionNames = new();

        #region Helpers
        private IMongoCollection<TEntity> TenantCollection<TEntity>() =>
            client.GetDatabase(tenantSelectionService.GetSelectedMongoTenant()).GetCollection<TEntity>(GetCollectionName<TEntity>());

        private IMongoCollection<TEntity> GlobalCollection<TEntity>() =>
            client.GetDatabase(tenantSelectionService.GetMongoGlobalDatabase()).GetCollection<TEntity>(GetCollectionName<TEntity>());

        /// <summary>Nome da collection = [Table("...")] da entidade (com cache). Sem atributo, usa o nome da classe.</summary>
        private static string GetCollectionName<TEntity>() =>
            _collectionNames.GetOrAdd(typeof(TEntity), t => t.GetCustomAttribute<TableAttribute>(true)?.Name ?? t.Name);
        #endregion

        #region Escrita (tenant)
        public Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity =>
            TenantCollection<TEntity>().InsertOneAsync(entity, cancellationToken: cancellationToken);

        public Task AddOrUpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity =>
            TenantCollection<TEntity>().ReplaceOneAsync(new BsonDocument("_id", entity.Id), entity, new ReplaceOptions { IsUpsert = true }, cancellationToken);

        /// <summary>
        /// Atualiza um campo via expressão (e => e.IsActive). O driver usa o [BsonElement] correto ("isActive"),
        /// evitando erro de digitação/caixa no nome do campo.
        /// </summary>
        public async Task<bool> UpdateFieldAsync<TEntity, TField>(FilterDefinition<TEntity> filter, Expression<Func<TEntity, TField>> field, TField value, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var result = await TenantCollection<TEntity>().UpdateOneAsync(filter, Builders<TEntity>.Update.Set(field, value), cancellationToken: cancellationToken);
            return result.IsAcknowledged && result.MatchedCount > 0;
        }

        public async Task<long> UpdateManyAsync<TEntity>(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity
        {
            var result = await TenantCollection<TEntity>().UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
            return result.ModifiedCount;
        }
        #endregion

        #region Leitura (tenant)
        public Task<List<TEntity>> TenantQuery<TEntity>(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            TenantCollection<TEntity>().Find(filter).Sort(sort).ToListAsync(cancellationToken);

        public Task<TEntity> TenantQuerySingle<TEntity>(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            TenantCollection<TEntity>().Find(filter).FirstOrDefaultAsync(cancellationToken);

        public Task<long> TenantCount<TEntity>(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            TenantCollection<TEntity>().CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        public Task<List<TEntity>> TenantQueryWithTake<TEntity>(FilterDefinition<TEntity> filter, int skip, int take, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            TenantCollection<TEntity>().Find(filter)
                .Sort(sort ?? Builders<TEntity>.Sort.Descending(e => e.Id)) // ordem estável para paginar
                .Skip(skip).Limit(take)
                .ToListAsync(cancellationToken);

        public Task<List<TProjection>> TenantProjection<TEntity, TProjection>(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TProjection> projection, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            TenantCollection<TEntity>().Find(filter).Sort(sort).Project(projection).ToListAsync(cancellationToken);
        #endregion

        #region Leitura (global)
        public Task<List<TEntity>> GlobalQuery<TEntity>(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            GlobalCollection<TEntity>().Find(filter).Sort(sort).ToListAsync(cancellationToken);

        public Task<TEntity> GlobalQuerySingle<TEntity>(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity =>
            GlobalCollection<TEntity>().Find(filter).FirstOrDefaultAsync(cancellationToken);
        #endregion
    }
}
