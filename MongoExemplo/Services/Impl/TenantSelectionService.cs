using Microsoft.Extensions.Configuration;

namespace MongoExemplo.Services.Impl
{
    /// <summary>
    /// Versão simplificada: no projeto real o tenant vem da requisição (token/header).
    /// Aqui ele começa com o "defaultTenant" do appsettings e pode ser trocado com SetSelectedTenant.
    /// Cada tenant vira um database separado: {prefixo}_{tenant}.
    /// </summary>
    public class TenantSelectionService(IConfiguration configuration) : ITenantSelectionService
    {
        private readonly string _prefix = BuildPrefix(configuration["mongoConnection:databasePrefixName"]);
        private string _tenant = configuration["mongoConnection:defaultTenant"] ?? "default";

        public string GetSelectedMongoTenant() => $"{_prefix}{_tenant}";

        public void SetSelectedTenant(string tenant) => _tenant = tenant;

        public string GetMongoGlobalDatabase() => $"{_prefix}{configuration["mongoConnection:globalDatabase"] ?? "Global"}";

        private static string BuildPrefix(string prefix) => string.IsNullOrWhiteSpace(prefix) ? string.Empty : $"{prefix}_";
    }
}
