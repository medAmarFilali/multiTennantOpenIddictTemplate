# Multi-Tenant OpenIddict Template

A .NET template showing how to build multi-tenant authentication using OpenIddict, Clean Architecture, and PostgreSQL.

## What is this?

If you're building a SaaS app or any multi-tenant platform and need OAuth2/OIDC authentication with OpenIddict, this template gives you a solid starting point. It shows how to handle tenant isolation properly, separate your auth server from your APIs, and structure everything using Clean Architecture.

The setup includes an auth server (OpenIddict) and a multi-tenant API, both ready to run via Docker Compose.

### Project Structure

```
├── src/
│   ├── Shared/
│   │   └── MultiTenancy/          # Shared multi-tenancy abstractions
│   ├── AuthServer/
│   │   ├── AuthServer.Domain/      # Domain entities and interfaces
│   │   ├── AuthServer.Application/ # Business logic and use cases
│   │   ├── AuthServer.Infrastructure/ # Data access and external services
│   │   └── AuthServer.API/         # HTTP API and entry point
│   └── TenantApi/
│       ├── TenantApi.Domain/
│       ├── TenantApi.Application/
│       ├── TenantApi.Infrastructure/
│       └── TenantApi.API/
├── tests/
│   ├── AuthServer.UnitTests/
│   ├── AuthServer.IntegrationTests/
│   ├── TenantApi.UnitTests/
│   ├── TenantApi.IntegrationTests/
│   └── MultiTenancy.Tests/
└── docs/                           # Detailed documentation
```

## How multi-tenancy works

The `tenant_id` claim in your JWT token is the source of truth. For unauthenticated requests, we resolve tenants via the `X-Tenant-Id` header or the `/t/{tenantKey}` URL prefix.

Data isolation uses a shared PostgreSQL database with a `TenantId` column on relevant tables. The design makes it straightforward to switch to schema-per-tenant or database-per-tenant later if needed.

## Running locally

**With Docker (easiest):**

```bash
docker-compose up
```

This spins up PostgreSQL (5432), AuthServer (5001), and TenantApi (5002).

**Without Docker:**

```bash
# Make sure PostgreSQL is running locally
dotnet run --project src/AuthServer/AuthServer.API
dotnet run --project src/TenantApi/TenantApi.API
```

Check `/docs/setup.md` for database migrations and detailed setup.

## Tests

```bash
dotnet test
```

All the important multi-tenancy bits have tests to verify tenant isolation works correctly.

## Tech stack

- ASP.NET Core 8
- OpenIddict (OAuth2/OIDC)
- PostgreSQL 16
- Entity Framework Core
- xUnit
- Docker

## Documentation

More details in the `/docs` folder:

- [Architecture Overview](docs/architecture.md)
- [Multi-Tenancy Strategy](docs/multi-tenancy.md)
- [Setup Guide](docs/setup.md)
- [Security Considerations](docs/security.md)
- [Deployment Guide](docs/deployment.md)

## Notes

This is a template, not a full product. There's no business logic or UI - just the auth and API infrastructure you need to build on top of. It's intentionally kept simple and focused.

## License

MIT
