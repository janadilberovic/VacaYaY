# CLAUDE.md

VacaYAY is a leave/vacation-management REST API. No public registration — HR provisions
accounts; employees request leave, HR reviews. **.NET 9, MySQL, JWT auth.**

## Architecture

Clean architecture, 4 projects. **Dependency direction (never violate):** references point inward
toward `Domain`, never outward. `Api → Business`; `Business → Data` and `Business → Domain`;
`Data → Domain`; `Domain` references no other project. `Api` is the composition root — all DI wiring
lives in `Program.cs`.

- `VacaYAY.Domain` — entities (`User`, `LeaveRequest`, `LeaveType`) + enums. No project refs.
- `VacaYAY.Data` — `VacaYAYDbContext`, EF migrations, MySQL config.
- `VacaYAY.Business` — services, DTOs, validators, Mapster configs (all app logic).
- `VacaYAY.Api` — controllers, `Program.cs`, JWT setup, `ClaimsPrincipalExtensions`, design-time factory.

## Commands

- Build: `dotnet build`
- Test: `dotnet test` (xUnit; only template stubs exist today)
- Run API: `dotnet run --project src/VacaYAY.Api` (http `:5266`, https `:7273`; Swagger in Development)
- **Migrations: do NOT create or apply them** (see Guardrails). Existing migrations live in
  `src/VacaYAY.Data/Migrations/`; the design-time factory is `src/VacaYAY.Api/VacaYAYDbContextFactory.cs`.

## Configuration & secrets

- MySQL connection: `ConnectionStrings:DefaultConnection` — **empty in `appsettings.json` by design**;
  the real value comes from user-secrets/env.
- JWT config lives in the `Jwt` section (`Issuer`, `Audience`, `AccessTokenHours`).
  **`Jwt:SigningKey` is NOT in appsettings** — set it via user-secrets (Program.cs fail-fast guard
  throws if it's missing or < 32 bytes):
  `dotnet user-secrets set "Jwt:SigningKey" "<32+ byte value>" --project src/VacaYAY.Api`
- `UserSecretsId` is on the Api project.
- `GITHUB_PAT` — fine-grained GitHub token (Contents + Pull requests, write) used by the `github`
  MCP server for `/pr`. **Environment variable only**, never in a file; set it as a Windows User
  variable and restart Claude Code.

## Conventions

- **Feature-folder layout** in Business: each concern gets a feature subfolder —
  `DTOs/<Feature>`, `Interfaces/<Feature>`, `Services/<Feature>`, `Validators/<Feature>`,
  `Mapping/<Feature>`. Namespaces mirror folders. Reference example: the `Auth` feature.
- **Services**: interface + impl paired 1:1 (`IAuthService` ↔ `AuthService`), constructor injection,
  all methods `async` with `CancellationToken cancellationToken = default`. Return DTOs, never
  entities. A null return means failure that the controller maps to a status code.
- **Controllers stay thin**: `[ApiController]`, route `api/<area>`; validate via injected
  `IValidator<T>.ValidateAsync` → `ValidationProblem` on failure; return RFC7807 `Problem(...)` for
  errors; delegate all logic to a Business service.
- **Validation**: FluentValidation `AbstractValidator<T>`, invoked manually in controllers (not auto
  model-validation). Register with `AddValidatorsFromAssemblyContaining<...>` in `Program.cs`.
- **Mapping**: **Mapster** (not AutoMapper). Add an `IRegister` config under `Mapping/<Feature>`; map
  with `.Adapt<T>()`. Configs are auto-scanned in `Program.cs`.
- **DI**: register new services inline in `Program.cs` (no `AddApplication()` extension pattern yet).
  Pick the lifetime deliberately (e.g. `TokenDenylist` = singleton).
- **EF model**: enums persisted as strings (`.HasConversion<string>().HasMaxLength(20)`); soft-delete
  via global query filters (`!IsDeleted`); FKs use `DeleteBehavior.Restrict` (no cascade). For a new
  entity, add a `DbSet` and configure it in `OnModelCreating` — but **do not generate the migration
  yourself** (see Guardrails).
- **Style**: `net9.0`, nullable + implicit usings enabled; `Async` suffix on async methods;
  `I`-prefixed interfaces.

## Auth model

- JWT bearer; the access token carries `sub` (user id), `email`, `role`, `jti`. Validation keeps
  original claim names (`MapInboundClaims = false`).
- Logout revokes the `jti` via `ITokenDenylist` (in-memory `IMemoryCache`, singleton, per-process
  only — flagged for Redis/DB if scaled out).
- Real passwords are hashed with ASP.NET Identity `PasswordHasher<User>`; first login uses a plaintext
  `TempPassword` compared with `FixedTimeEquals`.
- Authorization policy `"HrOnly"` = `RequireRole(HR)`. Read claims via `ClaimsPrincipalExtensions`
  (`GetUserId`, `GetRole`, `GetTokenId`, `GetExpiration`).

## Guardrails

- **Do** respect the `Api → Business → Data → Domain` dependency direction. Entities live in
  `VacaYAY.Domain` — a new entity goes there, never in Business, Data, or Api. Entities must not
  leak past Business: services return DTOs, and only DTOs cross into Api.
- **Do** use the single existing `DbContext` — `VacaYAYDbContext` in `VacaYAY.Data`. Never create a
  second `DbContext` (and never one in Api); add a `DbSet` to the existing one instead.
- **Do** keep controllers thin — logic goes in Business services.
- **Don't** create or apply EF migrations (`dotnet ef migrations add`, `dotnet ef database update`) —
  migrations are owned/managed outside Claude in this project. You may edit the EF model (entities,
  `OnModelCreating`); flag that a migration will be needed, but don't generate or run it.
- **Don't** hand-edit generated EF files: `Migrations/*.Designer.cs`, the migration `.cs` files, or
  `VacaYAYDbContextModelSnapshot.cs`.
- **Don't** put secrets in `appsettings*.json` (SigningKey, connection string) — use user-secrets.
- **Don't** return or log the `User` entity, `PasswordHash`, or `TempPassword` — return DTOs only.

## Quirks worth knowing

- DB is **MySQL (Pomelo)**, not SQL Server.
- Mapping is **Mapster**, not AutoMapper.
- `VacaYAYDbContext` inherits plain `DbContext` (it uses `IPasswordHasher<User>` but is not an
  Identity context).
- Domain enums currently sit in the **global namespace** (used unqualified across projects) — match
  that until deliberately changed.
- Minor: file `VacaYayDbContext.cs` vs type `VacaYAYDbContext` casing mismatch; some inline comments
  are in Serbian.
