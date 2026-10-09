# N3.Backend

ASP.NET Core Web API (.NET 10) with EF Core, split into four layers.

## Getting started

Requirements: .NET SDK 10.0 or newer.

```bash
dotnet restore N3.Backend.slnx
dotnet build N3.Backend.slnx
dotnet run --project N3.Backend.API
```

The API runs on `http://localhost:5079` (or `https://localhost:7106` with the `https` profile).
In Development, the OpenAPI document is available at `/openapi/v1.json`.
Example requests are in [N3.Backend.API.http](N3.Backend.API/N3.Backend.API.http).

## Solution structure

| Project | Responsibility | References |
|---|---|---|
| `N3.Backend.Domain` | Shared core types: exceptions, enums. No dependencies | None |
| `N3.Backend.Infrastructure` | The database: `ApplicationDbContext`, models, migrations. No dependencies on other projects | None |
| `N3.Backend.Application` | Business logic: services, DTOs, mapping, parameters | Domain, Infrastructure |
| `N3.Backend.API` | Entry point: controllers, exception handling, startup | Application, Infrastructure |

### Dependency direction

```
API  ->  Application  ->  Infrastructure
               |
               v
             Domain
```

- **Domain and Infrastructure depend on no other project.** Infrastructure only references the EF Core package.
- **Infrastructure contains only the database.** No services, repositories or DI extensions.
- **Database models never leave Application.** Services map them to DTOs before returning, so
  controllers only see DTOs.

### Folders

```
N3.Backend.Domain/
  Enums/
  Exceptions/                  NotFoundException
N3.Backend.Infrastructure/
  Database/
    ApplicationDbContext.cs
    Models/                    Company
  Migrations/                  EF Core migrations
N3.Backend.Application/
  DTOs/                        CompanyDto
  Extensions/                  AddApplication()
  Interfaces/                  ICompanyService
  Mappings/                    CompanyMapping (model -> DTO)
  Parameters/                  CompanyParameters (query-string filters)
  Services/                    CompanyService
N3.Backend.API/
  Controllers/                 CompanyController
  Middleware/                  GlobalExceptionHandler
  Program.cs                   Startup, DbContext registration
```

## Request flow

Using `GET /api/company/{id}` as the example:

```
HTTP request
  -> CompanyController       (API)             binds route/query input
  -> ICompanyService         (Application)
  -> CompanyService          (Application)     queries ApplicationDbContext
  -> ApplicationDbContext    (Infrastructure)  returns Company model
  <- CompanyService          throws NotFoundException if null, maps Company -> CompanyDto
  <- CompanyController       returns 200 with CompanyDto as JSON
```

- **Controllers** stay thin: bind input, call one service method, return `Ok(...)`. No try/catch.
- **Services** hold the business logic and query `ApplicationDbContext` directly. Use `AsNoTracking()` for reads.
- **Mapping** from model to DTO happens in Application.
- Pass `CancellationToken` all the way from the controller to the database call.

## Error handling

`GlobalExceptionHandler` turns exceptions into [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457) responses:

| Exception | Status |
|---|---|
| `NotFoundException` | 404 Not Found |
| Anything else | 500 Internal Server Error |

## Adding a new resource

Using `Company` as the template, a new resource `Foo` needs these files:

1. `Infrastructure/Database/Models/Foo.cs`, plus `public DbSet<Foo> Foos => Set<Foo>();` on `ApplicationDbContext`
2. `Application/Parameters/FooParameters.cs`: query filters
3. `Application/DTOs/FooDto.cs`: what the API returns
4. `Application/Mappings/FooMapping.cs`: `Foo` → `FooDto`
5. `Application/Interfaces/IFooService.cs` and `Application/Services/FooService.cs`
6. `API/Controllers/FooController.cs`
7. Register the service in `Application/Extensions/DependencyInjection.cs`:

```csharp
services.AddScoped<IFooService, FooService>();
```

8. Add a migration (see below).

## Migrations

Once a database provider is configured:

```bash
dotnet ef migrations add <Name> --project N3.Backend.Infrastructure --startup-project N3.Backend.API --output-dir Migrations
```

```bash
dotnet ef database update --project N3.Backend.Infrastructure --startup-project N3.Backend.API
```

Requires the `dotnet-ef` tool and the `Microsoft.EntityFrameworkCore.Design` package in `N3.Backend.API`.

## Naming conventions

| Type | Convention | Example |
|---|---|---|
| Controller | `{Resource}Controller`, route `api/[controller]` | `CompanyController` |
| Service | `I{Resource}Service` / `{Resource}Service` | `CompanyService` |
| Model | Singular table name | `Company` |
| DbSet | Plural | `Companies` |
| DTO | `{Resource}Dto` | `CompanyDto` |
| Parameters | `{Resource}Parameters` | `CompanyParameters` |
| Exception | `{What}Exception` | `NotFoundException` |
| Async methods | `Async` suffix | `GetCompaniesAsync` |

## Still to do

- Choose a database provider: add its package (e.g. `Microsoft.EntityFrameworkCore.SqlServer`) to Infrastructure and configure it in `Program.cs`.
- Set `ConnectionStrings:DefaultConnection`. Keep real connection strings in user secrets or environment variables, never in `appsettings.json`.
- Fill in `Company`, `CompanyDto`, `CompanyParameters` and `CompanyMapping.ToDto`. Until then, the Company endpoints return 500.
