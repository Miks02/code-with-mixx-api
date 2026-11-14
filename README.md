# CodeWithMixx API

CodeWithMixx is a platform for managing private IT tutoring: subjects, students, individual classes, project work, payments and inquiries. It replaces manual record keeping (messages, spreadsheets, notes) with a single system that has a public website for prospective students and an administrative dashboard for the instructor.

This repository contains the backend REST API. The web client is available at [code-with-mixx-web](https://github.com/Miks02/code-with-mixx-web).

## Purpose

- Provide public endpoints for the website, such as the subject catalog and the inquiry form.
- Provide secured endpoints for the administrative dashboard: subjects, students, class and project reservations, payments, inquiries and analytics.
- Handle authentication, account activation, password recovery and session management.
- Serve as a real world reference project for the technologies that are taught: from the database to the user interface.

## Technology Stack

- .NET 10, ASP.NET Core Minimal API
- Entity Framework Core with PostgreSQL (Npgsql)
- ASP.NET Core Identity, JWT Bearer authentication
- FluentValidation
- Scrutor
- Serilog (console, debug and Seq sinks)
- OpenAPI with Scalar
- Resend (transactional email)
- Handle authentication, account activation, password recovery and session management.
- Serve as a real world reference project for the technologies that are taught: from the database to the user interface.

## Technology Stack

- .NET 10, ASP.NET Core Minimal API
- Entity Framework Core with PostgreSQL (Npgsql)
- ASP.NET Core Identity, JWT Bearer authentication
- FluentValidation
- Scrutor
- Serilog (console, debug and Seq sinks)
- OpenAPI with Scalar
- Resend (transactional email)
- Discord.Net (inquiry notifications)
- xUnit, NSubstitute, AwesomeAssertions, MockQueryable

## Features

| Module               | Responsibility                                                                    |
| -------------------- | --------------------------------------------------------------------------------
| Authentication       | Login, logout, logout from all or a specific session, token rotation, account activation, password reset |
| Account              | Updating name, email, phone number and password                                   |
| Subjects             | Creating, updating, archiving, restoring and deleting subjects, public catalog and lookups |
| Students             | Student management, activation and deactivation, invitations, search and summaries |
| Reservations         | Class and project reservations, status changes, archiving and restoring, paged lists and summaries |
| Classes and Projects | Managing the individual classes and projects within a reservation
| PaymentTransactions  | Registering and subtracting payments per reservation                              |
| Inquiries            | Receiving inquiries from the public site, resolving and deleting them             |
| Analytics            | Aggregated data for the admin dashboard                                           |

## Architecture

### Project structure

```
CodeWithMixx.API/
  Common/          Shared abstractions (IEndpoint, IHandler, service interfaces)
                   and the Result pattern
  Domain/          Entities, value objects and domain errors
  Features/        Vertical slices, one folder per use case
  Infrastructure/  Persistence, security, rate limiting, email, Discord,
                   background jobs, filters, decorators and exception handlers
CodeWithMixx.UnitTests/
```

### Vertical slices

The API is organized by feature rather than by technical layer. Every use case lives in its own folder ans:

```
Features/Subjects/CreateSubject/
  CreateSubjectEndpoint.cs
  CreateSubjectHandler.cs
  CreateSubjectRequest.cs
  CreateSubjectResponse.cs
  CreateSubjectValidator.cs
```

Endpoints implement `IEndpoint` and are discovered by reflection at startup, then registered under the `/plement `IHandler<TRequest, TResponse>` and are registered automatically with Scrutor.

### Domain model

Business rules are enforced inside the entities rather than in handlers. Entities such as `Reservation` eand explicit mutation methods, keep their setters private and expose their collections as read only.Monetary values are represented by a `Money` value object. Each entity defines its own domain errors (for example `ReservationErrors`, `SubjectErrors`).

### Result pattern

Expected failures are not modeled as exceptions. Domain methods and handlers return a `Result` or `Result<T>` that carries either a value or a list of typed errors. Results are mapped to the appropriate typed HTTP responses at the endpoint level.

### Request pipeline

- **Validation:** an endpoint filter runs the FluentValidation validator for the request and returns a `400` response with an `errors` dictionary on failure.
- **Logging decorator:** every handler is wrapped in a decorator that logs the start, duration and outcome of the request, including error codes for failed results.
- **Exception handlers:** dedicated handlers cover unauthorized access, revoked tokens and security relatobal handler as the final fallback.

### Error handling

Every error response follows RFC 9457 ProblemDetails and carries an `errorCode` extension. Domain errors use their own codes (for example `Subject.AlreadyExists`), while all other errors receive a general code (for example `General.NotFound`, `General.InternalServerError`). This allows the client to map each error to a precise message.

### Persistence

- Entity Framework Core with PostgreSQL and per entity configuration classes.
- An audit interceptor sets `CreatedAt` and `UpdatedAt` automatically for every entity that implements `IAuditable`.
- Entities that implement `ISoftDeletable` support archiving and restoring instead of permanent deletion.
- A database seeder creates the initial admin account on startup.

### Security

- ASP.NET Core Identity with unique emails, account lockout and role based authorization policies (`AdminOnly`, `StudentOnly`).
- JWT access tokens and refresh tokens are delivered as HttpOnly cookies, so they are never accessible to client side JavaScript.
- Refresh tokens are stored as hashes, bound to a session, and record the IP address and user agent, which enables an overview of active sessions per device.
- Refresh tokens are rotated on every use. Reuse of an already consumed token revokes all sessions of that account.
- Student accounts are created through invitation links signed with a dedicated token provider.

### Rate limiting

A global limiter applies to all requests, with stricter dedicated policies for authentication, password reset, invitations and inquiries.

### Background jobs

- **CleanExpiredTokensJob:** periodically removes expired and revoked refresh tokens.
- **MarkOverdueReservationsJob:** periodically marks reservations with unpaid balances past their end dat

### Integrations

- **Resend:** sends account activation, password reset and email change messages.
- **Discord:** a bot posts new inquiries from the public site to a dedicated channel.

### Observability

Structured logging with Serilog, enriched with thread and process information. Request logs include the ae development environment the API exposes an OpenAPI document and a Scalar reference UI.

## Testing

Unit tests are written with xUnit, NSubstitute and AwesomeAssertions. They cover the Result pattern and its extensions, the token, cookie and user providers, and time handling.
