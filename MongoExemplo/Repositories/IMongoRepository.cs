using MongoDB.Driver;
using MongoExemplo.Entities.Base;
using System.Linq.Expressions;

namespace MongoExemplo.Repositories
{
    /// <summary>
    /// Repositório genérico: não conhece nenhuma entidade específica.
    /// Recebe FilterDefinition/SortDefinition/ProjectionDefinition prontos (vindos das Specifications).
    /// Tenant = database do cliente selecionado; Global = database compartilhado.
    /// </summary>
    public interface IMongoRepository
    {
        #region Escrita (tenant)
        Task AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task AddOrUpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<bool> UpdateFieldAsync<TEntity, TField>(FilterDefinition<TEntity> filter, Expression<Func<TEntity, TField>> field, TField value, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<long> UpdateManyAsync<TEntity>(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        #endregion

        #region Leitura (tenant)
        Task<List<TEntity>> TenantQuery<TEntity>(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<TEntity> TenantQuerySingle<TEntity>(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<long> TenantCount<TEntity>(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<List<TEntity>> TenantQueryWithTake<TEntity>(FilterDefinition<TEntity> filter, int skip, int take, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<List<TProjection>> TenantProjection<TEntity, TProjection>(FilterDefinition<TEntity> filter, ProjectionDefinition<TEntity, TProjection> projection, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        #endregion

        #region Leitura (global)
        Task<List<TEntity>> GlobalQuery<TEntity>(FilterDefinition<TEntity> filter, SortDefinition<TEntity> sort = null, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        Task<TEntity> GlobalQuerySingle<TEntity>(FilterDefinition<TEntity> filter, CancellationToken cancellationToken = default) where TEntity : BaseEntity;
        #endregion
    }
}
