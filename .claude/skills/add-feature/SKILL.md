---
name: add-feature
description: Scaffold a new VacaYAY Business feature slice (DTOs, interface, service, validators, Mapster config) plus a thin Api controller, mirroring the Auth reference feature. Use when the user asks to add a new feature/area/resource (e.g. "add a LeaveType feature", "scaffold LeaveRequest CRUD", "/add-feature <Name>").
---

# Add a VacaYAY feature slice

Scaffold a new feature `<X>` (e.g. `LeaveType`, `LeaveRequest`) across the Business and Api layers,
following the exact conventions the `Auth` feature already models. **Read the Auth reference files
first** — they are the source of truth for style, not this document:

- `src/VacaYAY.Api/Controllers/AuthController.cs`
- `src/VacaYAY.Business/Services/Auth/AuthService.cs`
- `src/VacaYAY.Business/Interfaces/Auth/IAuthService.cs`
- `src/VacaYAY.Business/Mapping/Auth/AuthMappingConfig.cs`
- `src/VacaYAY.Business/Validators/Auth/LoginRequestValidator.cs`
- `src/VacaYAY.Business/DTOs/Auth/` (DTO shapes)

## Steps

1. **Confirm the feature name and endpoints** with the user if not given. Ask what operations it
   needs (e.g. list / get / create / update / delete) and whether it introduces a **new entity**.

2. **Create the files** (replace `<X>` with the PascalCase feature name). Namespaces mirror folders.

   | Layer | Path |
   |---|---|
   | DTOs | `src/VacaYAY.Business/DTOs/<X>/` — request + response DTOs (records) |
   | Interface | `src/VacaYAY.Business/Interfaces/<X>/I<X>Service.cs` |
   | Service | `src/VacaYAY.Business/Services/<X>/<X>Service.cs` |
   | Validators | `src/VacaYAY.Business/Validators/<X>/<Request>Validator.cs` (one per request DTO that needs validation) |
   | Mapping | `src/VacaYAY.Business/Mapping/<X>/<X>MappingConfig.cs` (`: IRegister`) |
   | Controller | `src/VacaYAY.Api/Controllers/<X>Controller.cs` |

3. **Wire DI in Program.cs — exactly ONE line.** Add near the other service registrations
   (`src/VacaYAY.Api/Program.cs`, by the `AddScoped<IAuthService, AuthService>()` line):

   ```csharp
   builder.Services.AddScoped<I<X>Service, <X>Service>();
   ```

   Do **not** add anything else there:
   - **Validators auto-register** — `AddValidatorsFromAssemblyContaining<...>` already scans the whole
     Business assembly.
   - **Mapster auto-registers** — `TypeAdapterConfig.GlobalSettings.Scan(...)` already picks up any
     new `IRegister`.

   Pick the lifetime deliberately (scoped for anything touching `VacaYAYDbContext`).

4. **If a new entity is needed:** add the entity under `src/VacaYAY.Domain/Entities/`, add a `DbSet`
   and configure it in `OnModelCreating` in `src/VacaYAY.Data/VacaYayDbContext.cs` (enums as strings
   via `.HasConversion<string>().HasMaxLength(20)`, soft-delete query filter `!IsDeleted`, FKs with
   `DeleteBehavior.Restrict`). **Do NOT run `dotnet ef migrations add`** — migrations are owned
   outside Claude here. Tell the user a migration is needed and they must generate it.

5. **Build:** run `dotnet build` and fix any errors.

## Conventions to bake in (verified against Auth)

- **Service:** interface ↔ impl 1:1; constructor injection; every method `async` with
  `CancellationToken cancellationToken = default`. Return **nullable DTOs** — `null` means failure the
  controller maps to a status code. **Never return or log the entity**, `PasswordHash`, or
  `TempPassword`. Map entity → DTO with Mapster `.Adapt<T>()`.
- **Controller stays thin:** `[ApiController]`, `[Route("api/<x>")]`. Validate manually via injected
  `IValidator<T>.ValidateAsync(...)`; on failure return `ValidationProblem` (reuse the
  `ToValidationProblem` helper pattern from `AuthController.cs`). On service failure return an RFC7807
  `Problem(statusCode: ..., title: ...)`. Delegate all logic to the service — no business logic here.
- **Validator:** FluentValidation `AbstractValidator<TRequest>`.
- **Mapping:** `<X>MappingConfig : IRegister`, add `config.NewConfig<Entity, Dto>()` per pair.
- **Style:** file-scoped namespaces, `System.*` usings first, `Async` suffix on async methods,
  `I`-prefixed interfaces, `net9.0` nullable enabled.

## Respect the dependency direction
`Api → Business → Data → Domain`, never outward. Entities stay in Business/Data; only DTOs cross the
Api boundary.
