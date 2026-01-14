# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["VendorRiskScoringEngine.sln", "./"]

# Copy project files
COPY ["src/VendorRiskAPI.API/VendorRiskAPI.API.csproj", "src/VendorRiskAPI.API/"]
COPY ["src/VendorRiskAPI.Application/VendorRiskAPI.Application.csproj", "src/VendorRiskAPI.Application/"]
COPY ["src/VendorRiskAPI.Domain/VendorRiskAPI.Domain.csproj", "src/VendorRiskAPI.Domain/"]
COPY ["src/VendorRiskAPI.Infrastructure/VendorRiskAPI.Infrastructure.csproj", "src/VendorRiskAPI.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/VendorRiskAPI.API/VendorRiskAPI.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/VendorRiskAPI.API"
RUN dotnet build "VendorRiskAPI.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "VendorRiskAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/logs

# Copy published app
COPY --from=publish /app/publish .

# Copy seed data
COPY ["src/VendorRiskAPI.Infrastructure/Data/*.json", "./Data/"]

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:8080/api/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "VendorRiskAPI.API.dll"]
