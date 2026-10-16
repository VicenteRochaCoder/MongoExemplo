namespace MongoExemplo.Services
{
    public interface ITenantSelectionService
    {
        string GetSelectedMongoTenant();
        void SetSelectedTenant(string tenant);
        string GetMongoGlobalDatabase();
    }
}
