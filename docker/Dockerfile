# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder

WORKDIR /build

# Copy solution and project files
COPY src/ ./src/

# Restore and build
WORKDIR /build/src/DotNetApi
RUN dotnet restore
RUN dotnet build -c Release --no-restore
RUN dotnet publish -c Release -o /publish --no-build

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

# Copy certificate directory (or create placeholder)
COPY infrastructure/certs/ ./certs/ 2>/dev/null || true

# Copy published application
COPY --from=builder /publish .

# Expose ports
EXPOSE 80 443

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost/health/live || exit 1

# Run the application
ENTRYPOINT ["dotnet", "DotNetApi.dll"]
