# FindMyDevice API

A modern, clean architecture backend starter for SaaS applications built with .NET 10.

## Features
- **Clean Architecture**: Domain, Application, Infrastructure, and API layers.
- **Entity Framework Core**: Code-first approach with PostgreSQL support.
- **Pre-configured Auth**: JWT with Refresh Token support, Role-based authorization.
- **Service Patterns**: Generic interfaces for Email, Storage, and Auth providers.
- **OpenAPI/Scalar**: Built-in API documentation and testing UI.

## Getting Started
Ensure you have .NET 10 SDK installed.

```bash
dotnet restore
dotnet build FindMyDevice.slnx
dotnet test FindMyDevice.slnx
dotnet run --project src/FindMyDevice.API/FindMyDevice.API.csproj
```

