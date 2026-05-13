# FindMyDevice

نظام متكامل لتتبع وإدارة الأجهزة عن بُعد عند فقدانها أو سرقتها.

## المكونات
- **backend/** — ASP.NET Core 10 Web API (Clean Architecture)
- **android/** — تطبيق Kotlin + Jetpack Compose *(لاحقاً)*
- **web/** — لوحة تحكم Next.js 14 + TypeScript *(لاحقاً)*

## Backend Architecture
```
backend/
├── FindMyDevice.sln
└── src/
    ├── FindMyDevice.Domain/         ← Entities, Value Objects (لا تبعيات خارجية)
    ├── FindMyDevice.Application/    ← Use Cases, DTOs, Interfaces
    ├── FindMyDevice.Infrastructure/ ← EF Core, JWT, FCM, External services
    └── FindMyDevice.Api/            ← Controllers, SignalR Hubs, Middleware
```

اتجاه التبعيات:
`Api → Infrastructure → Application → Domain`

## Tech Stack
| Layer | Tech |
|---|---|
| Backend | ASP.NET Core 10, EF Core, SignalR |
| Database | PostgreSQL + PostGIS |
| Auth | JWT + Refresh Tokens |
| Push | Firebase Cloud Messaging |
| Mobile | Kotlin, Jetpack Compose, Hilt |
| Web | Next.js 14, TypeScript, TailwindCSS |

## Requirements
- .NET 10 SDK
- PostgreSQL 15+
- Android Studio (لاحقاً)
- Node.js 20+ (لاحقاً)

## Running the Backend
```bash
cd backend
dotnet build
dotnet run --project src/FindMyDevice.Api
```
