# MongoExemplo

Exemplo isolado do padrão **MongoRepository + Specification** usado no Netrin.Safepartner.BackgroundCheck.

## Estrutura

```
MongoExemplo/
├── CrossCutting/Bootstrap.cs                 # registra IMongoClient, ITenantSelectionService e IMongoRepository
├── Entities/
│   ├── Base/BaseEntity.cs, BaseAuditEntity.cs
│   └── Products/
│       ├── ProductEntity.cs                  # [Table("Products")] = nome da collection
│       ├── ProductEntitySpecification.cs     # filtros, ordenações, projeções e updates
│       └── Dtos/ProductSummaryDto.cs
├── Repositories/
│   ├── IMongoRepository.cs                   # repositório genérico (Tenant / Global)
│   └── Impl/MongoRepository.cs
├── Services/                                 # seleção do database do tenant
└── Program.cs                                # exemplos de uso
```

## Como rodar

```bash
docker compose up -d          # sobe um Mongo local na 27017
dotnet run --project MongoExemplo
```

## A ideia

- O **repositório** é genérico: só sabe executar `FilterDefinition`, `SortDefinition`, `ProjectionDefinition` e `UpdateDefinition`.
- A **Specification** de cada entidade concentra *o que* buscar. Quem chama só combina as peças:

```csharp
var filtro = ProductEntitySpecification.BySearch(category: "Informatica", maxPrice: 1000m);
var itens  = await repo.TenantQuery(filtro, ProductEntitySpecification.OrderByPriceAsc(), ct);

// também dá para combinar com operadores
var gamer = Spec.Active() & Spec.WithTag("gamer") & !Spec.OutOfStock();
```
