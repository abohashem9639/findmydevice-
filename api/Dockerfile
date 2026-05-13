# syntax=docker/dockerfile:1.7

# ──────────────────────────────────────────────────────────────────────────
# Stage 1: Build & Publish
# ──────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution metadata + shared props first — these rarely change, so the
# layer cache stays warm across most rebuilds.
COPY Directory.Build.props ./
COPY FindMyDevice.slnx ./

# Copy each csproj separately, then `restore`. Doing this BEFORE the source
# means dotnet restore can be cached as long as csproj files don't change.
COPY src/FindMyDevice.Domain/FindMyDevice.Domain.csproj           src/FindMyDevice.Domain/
COPY src/FindMyDevice.Application/FindMyDevice.Application.csproj src/FindMyDevice.Application/
COPY src/FindMyDevice.Infrastructure/FindMyDevice.Infrastructure.csproj src/FindMyDevice.Infrastructure/
COPY src/FindMyDevice.API/FindMyDevice.API.csproj                 src/FindMyDevice.API/

RUN dotnet restore src/FindMyDevice.API/FindMyDevice.API.csproj

# Now copy the rest of the source and publish.
COPY src/ src/

RUN dotnet publish src/FindMyDevice.API/FindMyDevice.API.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ──────────────────────────────────────────────────────────────────────────
# Stage 2: Runtime (slim)
# ──────────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# dotnet/aspnet:10.0 ships with a built-in non-root `app` user. Reuse it
# directly instead of trying to recreate it (which fails with exit code 9).
COPY --from=build --chown=app:app /app/publish .
USER app

# Koyeb (and most PaaS) forwards HTTP traffic to whatever port the container
# listens on. 8080 is the convention for non-privileged services.
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "FindMyDevice.API.dll"]
