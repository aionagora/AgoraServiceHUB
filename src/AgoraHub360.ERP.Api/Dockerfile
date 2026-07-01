# ──────────────────────────────────────────────────────────────────────
# Dockerfile — AgoraHub360 ERP API
# Build:     docker build -t agorahub360/erp-api -f src/AgoraHub360.ERP.Api/Dockerfile .
# Run:       docker run -p 7001:8080 agorahub360/erp-api
# ──────────────────────────────────────────────────────────────────────

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# ── Restore (capa cacheable) ──────────────────────────────────────────
COPY AgoraHub360.ERP.sln ./
COPY src/AgoraHub360.ERP.Domain/AgoraHub360.ERP.Domain.csproj src/AgoraHub360.ERP.Domain/
COPY src/AgoraHub360.ERP.Shared/AgoraHub360.ERP.Shared.csproj src/AgoraHub360.ERP.Shared/
COPY src/AgoraHub360.ERP.Application/AgoraHub360.ERP.Application.csproj src/AgoraHub360.ERP.Application/
COPY src/AgoraHub360.ERP.Persistence/AgoraHub360.ERP.Persistence.csproj src/AgoraHub360.ERP.Persistence/
COPY src/AgoraHub360.ERP.Infrastructure/AgoraHub360.ERP.Infrastructure.csproj src/AgoraHub360.ERP.Infrastructure/
COPY src/AgoraHub360.ERP.Api/AgoraHub360.ERP.Api.csproj src/AgoraHub360.ERP.Api/
COPY src/AgoraHub360.ERP.Web/AgoraHub360.ERP.Web.csproj src/AgoraHub360.ERP.Web/
RUN dotnet restore src/AgoraHub360.ERP.Api/AgoraHub360.ERP.Api.csproj

# ── Build ─────────────────────────────────────────────────────────────
COPY . .
RUN dotnet build src/AgoraHub360.ERP.Api/AgoraHub360.ERP.Api.csproj -c Release --no-restore

# ── Publish ───────────────────────────────────────────────────────────
RUN dotnet publish src/AgoraHub360.ERP.Api/AgoraHub360.ERP.Api.csproj -c Release -o /app/publish --no-restore

# ── Runtime ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Health check (requiere curl en la imagen)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Usar puerto 8080 por defecto (no privilegiado, estándar Cloud-Native)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --retries=3 --start-period=15s \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AgoraHub360.ERP.Api.dll"]
