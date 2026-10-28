# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

CodeWithMixx API is an ASP.NET Core 10 minimal-API backend (`CodeWithMixx.API`) for a tutoring platform: admins manage students, subjects, and reservations (class-based or project-based) with payment tracking; students are invited by email and log in with cookie-carried JWTs via ASP.NET Identity. Tests live in `CodeWithMixx.UnitTests` (xUnit).

## Commands

Build, restore, and run from the repo root (contains `CodeWithMixx.sln`):

```
dotnet build
dotnet run --project CodeWithMixx.API
dotnet test
```

Run a single test class or method:

```
dotnet test --filter "FullyQualifiedName~CodeWithMixx.UnitTests.Infrastructure.Security.TokenServiceTests"
dotnet test --filter "FullyQualifiedName~TokenServiceTests.AssignAuthTokens_ShouldReturnValidTokens"
```

EF Core migrations live in `CodeWithMixx.API/Infrastructure/Persistence/Migrations`:

```
dotnet ef migrations add <Name> --project CodeWithMixx.API --output-dir Infrastructure/Persistence/Migrations
dotnet ef database update --project CodeWithMixx.API
```

In Development, Scalar (`/scalar`) and the OpenAPI document are mapped. Enums are serialized as strings (`JsonStringEnumConverter` in `Program.cs`).

## Architecture

### Vertical slice feature organization

Endpoints live under `CodeWithMixx.API/Features/<Area>/<UseCase>/`, each folder a self-contained slice with (typically) an `Endpoint`, `Handler`, `Request`, `Response`, and FluentValidation `Validator`. There is no controller layer — endpoints are minimal-API route registrations. Types shared within an area (filter/sort enums, list item DTOs, filter resolvers) go in `Features/<Area>/Common/`.

Current areas: `Authentication`, `Students`, `Subjects`, `Reservations` (class + project reservations, archive/restore/delete, summaries, paged lists), `Classes` (archive/restore a single class inside a reservation), `Projects` (details/update of a single project inside a reservation), `PaymentTransactions` (register/subtract payments, list by reservation).

- **Endpoint**: implements `IEndpoint` (`Common/Interfaces/IEndpoint.cs`) with a `MapEndpoint(IEndpointRouteBuilder)` method that calls `app.MapPost/Get/...`, takes the request and the matching `IHandler<TRequest, TResponse>` as lambda parameters, and converts the `Result`/`Result<T>` via `ResultExtensions.ToTypedResult(...)`. Endpoints chain `.RequireAuthorization("AdminOnly")` (when protected), `.WithTags("<Area>")`, and optionally `.RequireRateLimiting(...)` / `.Produces...()`.
- **Routes**: everything is under `api/`. Admin-only endpoints use an `admin/` prefix (e.g. `admin/reservations/classes/{reservationId:int}`); sub-entities of a reservation use `items/` (e.g. `admin/reservations/projects/items/{projectId:int}`, `admin/classes/items/{classId:int}/archive`). Archive/restore are `POST .../archive` and `POST .../restore`.
- **Handler**: implements `IHandler<TRequest, TResponse>` (`Common/Interfaces/IHandler.cs`) with `HandleAsync(TRequest, CancellationToken)`. Handlers talk directly to `AppDbContext` (no repository layer) and call domain entity factories/methods. Common injected services: `IUserProvider` (current user id/IP/roles), `IUserClock` (client-local "today"), `IAuthEmailSender`.
- **Discovery/registration**: `Infrastructure/EndpointMapper.cs` reflects over the assembly at startup for all `IEndpoint` implementations and maps them under an `api/` route group with `ValidationFilter` and `ProblemDetailsFilter` applied. `Infrastructure/DependencyInjection.cs` uses Scrutor (`services.Scan`) to register every `IHandler<,>` implementation as scoped, then wraps all of them with `LoggingDecorator<,>` (`Infrastructure/Decorators/LoggingDecorator.cs`), which logs start/success/failure/exception for every handler — no per-handler logging needed.
- New feature = new folder under `Features/`; no manual registration required.

### Result pattern (no exceptions for expected failures)

`Common/Results/Result.cs` defines `Result` / `Result<T>` with `IsSuccess`, `Errors`, and `Payload`. Domain and handler code returns `Result.Failure(...)`/`Result<T>.Success(...)` instead of throwing for business failures; failures are propagated with `Result<T>.Failure(innerResult.Errors[0])`. `Common/Results/PagedResult.cs` provides `PagedResult<T>.CreateAsync(query, pageNumber, pageSize, ct)` for paged list endpoints (page size clamped to 1–100).

`Common/Results/Error.cs` defines `Error(Code, Description, ErrorType)` — note its namespace is `CodeWithMixx.API.Common.Result` (singular), unlike the rest of the folder. `ErrorType` (`Failure`, `Validation`, `NotFound`, `Unauthorized`, `Forbidden`, `Conflict`, `TooManyRequests`) drives the HTTP status in `ResultExtensions.ToProblemResult`. Each domain area has an error catalog of static factory methods in `Domain/Entities/<Area>/<Area>Errors.cs` (class named `<Area>Error`, e.g. `ReservationError.InvalidAmount(amount)`, `StudentError.NotFound(id)`, `MoneyError.NegativeValue()`); auth errors are in `Domain/ErrorCatalog/AuthError.cs`. `HandleIdentityResult` maps `IdentityResult` errors onto these catalogs.

`result.ToTypedResult(HttpStatusCode.Created, location)` maps success to the given status and failure (first error only) to an RFC7807 `ProblemDetails` with `type = urn:codewithmixx-api:error:<Code>` and an `errorCode` extension. `AddCustomProblemDetails` in `DependencyInjection.cs` gives framework-generated problems the same shape (`General.*` codes). Unhandled exceptions flow through `Infrastructure/Exceptions/Handlers/` (`TokensRevokedExceptionHandler`, `SecurityDbUpdateExceptionHandler`, `GlobalExceptionHandler`, in that order).

### Domain model

Entities have private constructors/setters and expose static factories plus behavior methods returning `Result`/`Result<T>`; child collections are private `List<T>` exposed as `IReadOnlyCollection<T>`. `*CreateData`/`*UpdateData` records (e.g. `ReservationCreateData`, `ClassCreateData`, `ProjectCreateData`, `ProjectUpdateData`) decouple entity input from HTTP request shapes.

- `User : IdentityUser` (with `AccountStatus`) has a 1:1 `Student` or `Admin` profile keyed by `UserId`.
- `Reservation` (`Domain/Entities/Reservations/Reservation.cs`) is the central aggregate. `ReservationType` is `Class` or `Project`; it owns either `Classes` or `Projects`, plus `PaymentTransactions`. Created via `CreateClassReservation` / `CreateProjectReservation`; changes go through methods like `UpdateClasses`, `UpdateProjects`, `RegisterPayment`, `SubtractPayment`, `UpdateTotalPrice`, which recalculate `TotalPrice`, `DiscountRate`, `Bonus`, and `PaymentStatus` internally. Never set these fields from handlers.
- Methods whose outcome depends on the date take a `DateOnly today` argument; handlers pass `IUserClock.Today` (resolved from the `TimeZoneId` request header, falling back to `Europe/Belgrade`).
- `Money` (`Domain/ValueObjects/Monies/`) is a non-negative, 2-decimal value object used for payment transaction amounts.

### Validation

FluentValidation validators (`<UseCase>Validator.cs`) are auto-registered via `AddValidatorsFromAssembly`. `Infrastructure/Filters/ValidationFilter.cs` is a global endpoint filter that resolves an `IValidator<T>` for each endpoint argument and short-circuits with `TypedResults.ValidationProblem(...)` on failure — no per-endpoint wiring. Validators can inject services (e.g. `IUserClock`). Domain methods re-check invariants, so validators are a first line, not the only one.

### Auth

ASP.NET Identity + JWT bearer auth, with the JWT read from the `AccessToken` cookie (`SecurityRegistration.cs`, `JwtBearerEvents.OnMessageReceived`) instead of the `Authorization` header; cookies are set/cleared via `ICookieProvider`. Refresh tokens are persisted (`Domain/Entities/RefreshTokens/`) and rotated through `Features/Authentication/RotateTokens/`. Policies: `AdminOnly`, `StudentOnly`.

Student onboarding: admin creates a student (`CreateStudent`, `AccountStatus.Pending`) → `SendInvitation` emails a link with a token from the custom `InviteTokenProvider` (7-day lifespan, `InviteTokenProviderOptions.cs`) → student sets a password via `Authentication/ActivateAccount`. Password reset uses the default Identity token provider (1-hour lifespan). The initial admin and roles are seeded on startup by `DatabaseSeeder` (`app.MapSeeders()`, config section `Admin`).

### Persistence

EF Core + Npgsql (PostgreSQL). `AppDbContext` (`Infrastructure/Persistence/AppDbContext.cs`) extends `IdentityDbContext<User>` (Identity tables renamed to `Users`, `Roles`, ...) and applies `IEntityTypeConfiguration<T>` classes from `Infrastructure/Persistence/Configurations/`. `PersistenceRegistration.cs` builds the connection string from `DATABASE_URL` (Railway) when set, otherwise `ConnectionStrings:DefaultConnection`.

- **Soft delete**: every `ISoftDeletable` entity gets a global `!IsDeleted` query filter. "Archive"/"restore" endpoints soft-delete/un-delete through entity methods; handlers that need archived rows use `.IgnoreQueryFilters()`. `DeleteReservation` is a real hard delete, blocked by `Reservation.RequiresHistoryRetention(today)`.
- **Auditing**: `Interceptors/AuditInterceptor.cs` sets `CreatedAt`/`UpdatedAt` on `IAuditable` entities on save (via `TimeProvider`) — don't set them manually. Bulk `ExecuteUpdateAsync` bypasses the interceptor, so set `UpdatedAt` explicitly there.

### Background jobs

`Infrastructure/BackgroundServices/` contains `BackgroundService`s using `PeriodicTimer` and a fresh DI scope per tick, registered in `DependencyInjection.cs`:
- `CleanExpiredTokensJob` — deletes expired refresh tokens (`RefreshConfig:RetentionThresholdInDays`, `RefreshConfig:CleanupIntervalInDays`).
- `MarkOverdueReservationsJob` — bulk-sets `PaymentStatus.Overdue` on unpaid reservations whose last class/project has ended (`BackgroundServices:MarkOverdueReservationsJob:IntervalInDays`).

### Cross-cutting infrastructure

- Email: Resend (`Infrastructure/Emails/`), exposed as `IAuthEmailSender` (invitation, password reset, email change, account activated/deactivated/deleted). Templates are inline HTML in Serbian; links point to `Web:ClientUrl`.
- Rate limiting (`Infrastructure/RateLimiting/`): global per-IP token bucket plus named policies `AuthLimiter`, `ForgotPasswordLimiter`, `InvitationLimiter` applied per-endpoint with `.RequireRateLimiting(...)`. Rejections return a `ProblemDetails` 429.
- CORS: `Infrastructure/Cors/CorsRegistration.cs` defines `DevCors` (hardcoded `https://localhost:4200`) — dev-only, needs a production-aware policy before shipping.
- Logging: Serilog from `appsettings.json` (Console/Debug/Seq sinks); request logging is enriched with the user id. `ProblemDetailsFilter` logs every problem response with its error code.
- All wiring is centralized in `Infrastructure/DependencyInjection.cs::AddInfrastructure`, with per-concern `Add*` extension methods (`AddPersistence`, `AddSecurity`, `AddEmailServices`, `AddUtility`, rate limiters, CORS).
- Config sections: `ConnectionStrings`, `JwtConfig`, `RefreshConfig`, `CookieConfig`, `Resend`, `Web`, `Admin`, `BackgroundServices`, `Serilog`.

## Testing conventions

- xUnit + AwesomeAssertions (`.Should()`) + NSubstitute + `Microsoft.EntityFrameworkCore.InMemory` for `AppDbContext` + `MockQueryable` where the in-memory provider isn't a good fit.
- Test class per SUT, named `<Type>Tests`, mirroring the API folder structure, with the system under test built in the constructor and assigned to `_sut`.
- Existing coverage: `Common/Results/`, `Infrastructure/Security/`, `Infrastructure/Utilities/` (`UserClock`). Domain entities and `Features/*` handlers do not yet have unit tests.
