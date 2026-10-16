using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoExemplo.CrossCutting;
using MongoExemplo.Entities.Products;
using MongoExemplo.Repositories;
using Spec = MongoExemplo.Entities.Products.ProductEntitySpecification;

// ContentRoot = pasta do executável, para achar o appsettings.json de qualquer lugar onde rodar
var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory });
builder.Services.RegisterMongo(builder.Configuration);

using var host = builder.Build();
using var scope = host.Services.CreateScope();
var repo = scope.ServiceProvider.GetRequiredService<IMongoRepository>();
var ct = CancellationToken.None;

// 1) Inserir alguns produtos (AddOrUpdate = upsert pelo _id, então pode rodar várias vezes)
var produtos = new List<ProductEntity>
{
    new() { Id = "p1", Name = "Notebook Pro 14",  Category = "Informatica", Price = 7500m, Stock = 4,  Tags = ["premium"] },
    new() { Id = "p2", Name = "Mouse sem fio",    Category = "Informatica", Price = 120m,  Stock = 50, Tags = ["acessorio"] },
    new() { Id = "p3", Name = "Teclado mecanico", Category = "Informatica", Price = 450m,  Stock = 0,  Tags = ["acessorio", "gamer"] },
    new() { Id = "p4", Name = "Cadeira gamer",    Category = "Moveis",      Price = 1300m, Stock = 7,  Tags = ["gamer"] },
};
foreach (var p in produtos)
    await repo.AddOrUpdateAsync(p, ct);

// 2) Buscar um pelo Id
var notebook = await repo.TenantQuerySingle(Spec.ById("p1"), ct);
Console.WriteLine($"ById(p1): {notebook?.Name}");

// 3) Filtro composto vindo da Specification + ordenação
var filtro = Spec.BySearch(category: "Informatica", maxPrice: 1000m);
var baratos = await repo.TenantQuery(filtro, Spec.OrderByPriceAsc(), ct);
Console.WriteLine($"Informatica ate R$1000: {string.Join(", ", baratos.Select(p => $"{p.Name} ({p.Price:C})"))}");

// 4) Combinando specifications com & e |
var gamerComEstoque = Spec.Active() & Spec.WithTag("gamer") & !Spec.OutOfStock();
Console.WriteLine($"Gamer com estoque: {await repo.TenantCount(gamerComEstoque, ct)}");

// 5) Paginação
var pagina1 = await repo.TenantQueryWithTake(Spec.Active(), skip: 0, take: 2, Spec.OrderByPriceAsc(), ct);
Console.WriteLine($"Pagina 1: {string.Join(", ", pagina1.Select(p => p.Name))}");

// 6) Projeção: traz só Id, Name e Price
var resumo = await repo.TenantProjection(Spec.BySearch(name: "mouse"), Spec.SummaryProjection(), cancellationToken: ct);
Console.WriteLine($"Projecao 'mouse': {string.Join(", ", resumo.Select(r => $"{r.Id}:{r.Name}"))}");

// 7) Updates
await repo.UpdateFieldAsync(Spec.ById("p3"), e => e.Stock, 10, ct);              // campo único
await repo.UpdateManyAsync(Spec.ByCategory("Moveis"), Spec.AddStock(1), ct);      // update da Specification
Console.WriteLine($"Estoque p3 agora: {(await repo.TenantQuerySingle(Spec.ById("p3"), ct)).Stock}");
