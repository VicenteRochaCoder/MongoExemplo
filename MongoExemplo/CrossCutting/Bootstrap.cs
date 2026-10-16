using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoExemplo.Repositories;
using MongoExemplo.Services;
using MongoExemplo.Services.Impl;

namespace MongoExemplo.CrossCutting
{
    public static class Bootstrap
    {
        public static IServiceCollection RegisterMongo(this IServiceCollection services, IConfiguration configuration)
        {
            ConventionRegistry.Register("Ignore Extra Elements Convention",
                new ConventionPack { new IgnoreExtraElementsConvention(true) }, _ => true);

            var settings = MongoClientSettings.FromConnectionString(configuration["mongoConnection:server"]);
            services.AddSingleton<IMongoClient>(_ => new MongoClient(settings));

            services.AddScoped<ITenantSelectionService, TenantSelectionService>();
            services.AddScoped<IMongoRepository, Repositories.Impl.MongoRepository>();

            return services;
        }
    }
}
