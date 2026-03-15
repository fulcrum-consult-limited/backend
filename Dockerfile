# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY *.sln ./
COPY src/Shared/Shared.Domain/Shared.Domain.csproj src/Shared/Shared.Domain/
COPY src/Shared/Shared.Application/Shared.Application.csproj src/Shared/Shared.Application/
COPY src/Services/Identity/Identity.Domain/Identity.Domain.csproj src/Services/Identity/Identity.Domain/
COPY src/Services/Identity/Identity.Application/Identity.Application.csproj src/Services/Identity/Identity.Application/
COPY src/Services/Identity/Identity.Infrastructure/Identity.Infrastructure.csproj src/Services/Identity/Identity.Infrastructure/
COPY src/Services/Identity/Identity.API/Identity.API.csproj src/Services/Identity/Identity.API/

# Restore dependencies
RUN dotnet restore src/Services/Identity/Identity.API/Identity.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish src/Services/Identity/Identity.API/Identity.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user — Debian syntax
RUN groupadd --system --gid 1001 appgroup && \
    useradd --system --uid 1001 --gid appgroup appuser

# Copy published output
COPY --from=build /app/publish .

# Set ownership
RUN chown -R appuser:appgroup /app

USER appuser

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Identity.API.dll"]