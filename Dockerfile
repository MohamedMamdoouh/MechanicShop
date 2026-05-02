# ----- Build Stage -----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy project files for restore
COPY ["src/MechanicShop.Api/MechanicShop.Api.csproj", "src/MechanicShop.Api/"]
COPY ["src/MechanicShop.Application/MechanicShop.Application.csproj", "src/MechanicShop.Application/"]
COPY ["src/MechanicShop.Domain/MechanicShop.Domain.csproj", "src/MechanicShop.Domain/"]
COPY ["src/MechanicShop.Contracts/MechanicShop.Contracts.csproj", "src/MechanicShop.Contracts/"]
COPY ["src/MechanicShop.Infrastructure/MechanicShop.Infrastructure.csproj", "src/MechanicShop.Infrastructure/"]
COPY ["Directory.Packages.props", "."]
COPY ["Directory.Build.props", "."]

# Restore dependencies (only Api project — it references all others)
RUN dotnet restore "src/MechanicShop.Api/MechanicShop.Api.csproj"

# Copy source code
COPY . .

# Publish — skip restore since it was done above
RUN dotnet publish "src/MechanicShop.Api/MechanicShop.Api.csproj" \
    --configuration Release \
    --no-restore \
    --output /app

# ----- Final Stage -----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Install timezone data and curl (healthcheck) in a single layer, so keep image small
RUN apt-get update && \
    apt-get install -y --no-install-recommends tzdata curl && \
    ln -fs /usr/share/zoneinfo/Africa/Cairo /etc/localtime && \
    dpkg-reconfigure -f noninteractive tzdata && \
    rm -rf /var/lib/apt/lists/*

ENV TZ=Africa/Cairo

WORKDIR /app

# Copy published output owned by the built-in non-root 'app' user
COPY --from=build --chown=app:app /app .

# Run as non-root — required for security (OWASP A05)
USER app

# Port 8080: non-root users cannot bind to privileged ports
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "MechanicShop.Api.dll"]
