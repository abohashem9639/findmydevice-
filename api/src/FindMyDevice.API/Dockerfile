# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

# 1. BASE STAGE (Runtime)
WORKDIR /app
# Security: Use the built-in 'app' user explicitly (Standard in .NET 8+)
USER app
# Network: Only expose HTTP (8080). Caddy handles HTTPS (443) -> HTTP (8080)
EXPOSE 8080
# EXPOSE 8081 <-- HTTPs handled by Caddy, no need to expose here

# 2. BUILD STAGE
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG BUILD_COMMIT_SHA=""
ARG BUILD_TIMESTAMP_UTC=""
WORKDIR /src
# Copy root properties and solution configurations
COPY Directory.Build.props .
# Restore dependencies strictly (Optimizes caching and removes warnings)
COPY ["src/Sporestec.BuildFast.Application/Sporestec.BuildFast.Application.csproj", "src/Sporestec.BuildFast.Application/"]
COPY ["src/Sporestec.BuildFast.Domain/Sporestec.BuildFast.Domain.csproj", "src/Sporestec.BuildFast.Domain/"]
COPY ["src/Sporestec.BuildFast.Infrastructure/Sporestec.BuildFast.Infrastructure.csproj", "src/Sporestec.BuildFast.Infrastructure/"]
COPY ["src/Sporestec.BuildFast.API/Sporestec.BuildFast.API.csproj", "src/Sporestec.BuildFast.API/"]
RUN dotnet restore "./src/Sporestec.BuildFast.API/Sporestec.BuildFast.API.csproj"
# Build the app natively
COPY src/ src/
WORKDIR "/src/src/Sporestec.BuildFast.API"
RUN dotnet build "./Sporestec.BuildFast.API.csproj" -c $BUILD_CONFIGURATION -o /app/build \
    /p:SourceRevisionId=$BUILD_COMMIT_SHA \
    /p:BuildTimestampUtc=$BUILD_TIMESTAMP_UTC

# 3. PUBLISH STAGE
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG BUILD_COMMIT_SHA=""
ARG BUILD_TIMESTAMP_UTC=""
RUN dotnet publish "./Sporestec.BuildFast.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false \
    /p:SourceRevisionId=$BUILD_COMMIT_SHA \
    /p:BuildTimestampUtc=$BUILD_TIMESTAMP_UTC

# 4. FINAL STAGE
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sporestec.BuildFast.API.dll"]
