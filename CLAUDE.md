# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

CodeWithMixx API is an ASP.NET Core 10 minimal-API backend (`CodeWithMixx.API`) for managing students, subjects, classes, and reservations, with cookie-carried JWT authentication via ASP.NET Identity. Tests live in `CodeWithMixx.UnitTests` (xUnit).

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

EF Core migrations (run from `CodeWithMixx.API`, or pass `--project`/`--startup-project` from the root):

```
dotnet ef migrations add <Name> --project CodeWithMixx.API
dotnet ef database update --project CodeWithMixx.API
```

## Architecture

### Vertical slice feature organization

Endpoints live under `CodeWithMixx.API/Features/<Area>/<UseCase>/`, each folder a self-contained slice with (typically) an `Endpoint`, `Handler`, `Request`, `Response`, and FluentValidation `Validator`. There is no controller layer — endpoints are minimal-API route registrations.

- **Endpoint**: implements `IEndpoint` (`Common/Interfaces/IEndpoint.cs`) with a `MapEndpoint(IEndpointRouteBuilder)` method that calls `app.MapPost/Get/...`, resolves the matching `IHandler<TRequest, TResponse>` from DI per-request, and converts the `Result`/`Result<T>` to an `IResult` via `ResultExtensions.ToTypedResult(...)`.
- **Handler**: implements `IHandler<TRequest, TResponse>` (`Common/Interfaces/IHandler.cs`) with `HandleAsync(TRequest, CancellationToken)`. Handlers talk directly to `AppDbContext` (no repository layer) and to domain entity static factory methods.
- **Discovery/registration**: `Infrastructure/EndpointMapper.cs` reflects over the assembly at startup for all `IEndpoint` implementations and maps them under an `api/` route group with `ValidationFilter` and `ProblemDetailsFilter` applied globally. `Infrastructure/DependencyInjection.cs` uses Scrutor (`services.Scan`) to register every `IHandler<,>` implementation with a scoped lifetime, then wraps all of them with `LoggingDecorator<,>` (`Infrastructure/Decorators/LoggingDecorator.cs`), which logs start/success/failure/exception for every handler invocation automatically — no per-handler logging needed.
- New feature = new folder under `Features/`; no manual registration required beyond adding the files (endpoint mapping and handler DI both happen via reflection/scanning).

### Result pattern (no exceptions for expected failures)

`Common/Results/Result.cs` defines `Result` / `Result<T>` with `IsSuccess`, `Errors` (list of `Error`), and `Payload` (generic variant). Domain and handler code returns `Result.Failure(...)`/`Result<T>.Success(...)` instead of throwing for expected/business failures. `Common/Results/Error.cs` defines `Error(Code, Description, ErrorType)` where `ErrorType` (`Failure`, `Validation`, `NotFound`, `Unauthorized`, `Forbidden`, `Conflict`, `TooManyRequests`) drives the HTTP status mapping in `ResultExtensions.ToProblemResult`. Each domain area defines its own error catalog as static factory methods (e.g. `StudentError.NotFound(id)`, `SubjectError.MultipleSubjectsMissing(ids)`, `ReservationError.InvalidAmount(amount)`) under `Domain/Entities/<Area>/` or `Domain/ErrorCatalog/`.

Endpoints convert a `Result`/`Result<T>` to an HTTP response with `result.ToTypedResult(HttpStatusCode.Created, location)` — success maps to the given status code, failure maps to an RFC7807 `ProblemDetails` response using `error.Type`. Unhandled exceptions instead flow through `Infrastructure/Exceptions/Handlers/` (`GlobalExceptionHandler`, `SecurityDbUpdateExceptionHandler`, `TokensRevokedExceptionHandler`), registered via `IExceptionHandler`.

### Domain entities own their invariants

Entities (e.g. `Reservation`, `Class`) have private constructors and expose `static Result<T> Create...(...)` factory methods that validate input and return a `Result<T>` rather than throwing — see `Domain/Entities/Reservations/Reservation.cs::CreateClassReservation`. Handlers call these factories and propagate failures via `Result<T>.Failure(...)`. `*CreateData` records (e.g. `ReservationCreateData`, `ClassCreateData` in `Domain/Entities/Reservations/`) are the input DTOs passed into these factories, decoupling entity creation from the HTTP request shape.

### Validation

FluentValidation validators (`<UseCase>Validator.cs`) are auto-registered via `AddValidatorsFromAssembly` in `DependencyInjection.cs`. `Infrastructure/Filters/ValidationFilter.cs` is a global `IEndpointFilter` that reflects over each minimal-API argument, resolves a matching `IValidator<T>` if one is registered, and short-circuits with `TypedResults.ValidationProblem(...)` on failure — so validators run automatically for any endpoint whose request type has a validator; no per-endpoint wiring needed.

### Auth

ASP.NET Identity (`User : IdentityUser`, roles) + JWT bearer auth, but the JWT is read from an `AccessToken` cookie (`SecurityRegistration.cs`, `JwtBearerEvents.OnMessageReceived`) rather than the `Authorization` header — cookies are set/cleared via `ICookieProvider`/`CookieProvider`. Refresh tokens are persisted (`Domain/Entities/RefreshTokens/RefreshToken.cs`) and rotated through `Features/Authentication/RotateTokens/`. Authorization policies are role-based: `AdminOnly`, `StudentOnly` (`SecurityRegistration.cs`), applied per-endpoint via `.RequireAuthorization("PolicyName")`. `IUserProvider` (`Infrastructure/Security/UserProvider.cs`) exposes the current request's user id/IP from `HttpContext` for use in handlers (e.g. stamping `AdminId` on a new reservation).

### Persistence

EF Core + Npgsql (PostgreSQL). `AppDbContext` (`Infrastructure/Persistence/AppDbContext.cs`) plus per-entity `IEntityTypeConfiguration<T>` classes under `Infrastructure/Persistence/Configurations/`. `IAuditable` (`CreatedAt`/`UpdatedAt`) and `ISoftDeletable` are shared interfaces some entities implement; check `AppDbContext` for global query filters/interceptors before assuming a delete is a hard delete. `DatabaseSeeder` seeds roles and an initial admin on startup (`Program.cs` → `app.MapSeeders()`).

### Cross-cutting infrastructure

- Rate limiting: `Infrastructure/RateLimiting/GlobalRateLimiter.cs` (per-IP token bucket, applied globally) and `AuthRateLimiter.cs` (tighter limits for auth endpoints), both returning a `ProblemDetails` 429 body.
- CORS: `Infrastructure/Cors/CorsRegistration.cs` defines a `DevCors` policy (currently hardcoded to `https://localhost:4200`) — this is dev-only and will need a production-aware policy before shipping.
- Logging: Serilog, configured from `appsettings.json` (`Log.UseSerilog(... ReadFrom.Configuration ...)`); `ProblemDetailsFilter` additionally logs every problem-result response with its error code.
- All infrastructure wiring is centralized in `Infrastructure/DependencyInjection.cs::AddInfrastructure`, called once from `Program.cs`.

## Testing conventions

- xUnit + AwesomeAssertions (fluent `.Should()` assertions) + NSubstitute (mocking) + `Microsoft.EntityFrameworkCore.InMemory` for `AppDbContext` in tests + `MockQueryable` for mocking `IQueryable`/`DbSet` where the in-memory provider isn't a good fit.
- Test class per SUT, named `<Type>Tests`, with the system under test built in the constructor and assigned to `_sut`.
- Existing coverage is concentrated in `Common/Results/` and `Infrastructure/Security/` — most `Features/*` handlers do not yet have unit tests.
