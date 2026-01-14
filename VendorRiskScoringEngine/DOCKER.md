# Docker Deployment Guide

## Quick Start

### Using Docker Compose (Recommended)

```bash
# Start all services (API + PostgreSQL + Redis)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

**API will be available at:** http://localhost:5001

---

## Services

### 1. API Service
- **Container:** vendorrisk-api
- **Port:** 5001 (maps to 8080 inside container)
- **Environment:** Development
- **Health Check:** http://localhost:5001/api/health

### 2. PostgreSQL Database
- **Container:** vendorrisk-postgres
- **Port:** 5432
- **Database:** VendorRiskDB
- **Username:** vendorrisk
- **Password:** VendorRisk2024!

### 3. Redis Cache
- **Container:** vendorrisk-redis
- **Port:** 6379

---

## Building Manually

### Build Image
```bash
docker build -t vendorrisk-api:latest .
```

### Run Container
```bash
docker run -d \
  --name vendorrisk-api \
  -p 5001:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Database=VendorRiskDB;Username=postgres;Password=postgres" \
  vendorrisk-api:latest
```

---

## Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment (Development/Production) | Production |
| `ASPNETCORE_URLS` | Listening URLs | http://+:8080 |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | - |
| `Redis__ConnectionString` | Redis connection string | localhost:6379 |
| `Redis__InstanceName` | Redis key prefix | VendorRisk_ |

---

## Docker Compose Commands

### Start Services
```bash
# Start in background
docker-compose up -d

# Start with rebuild
docker-compose up -d --build

# Start specific service
docker-compose up -d api
```

### View Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres

# Last 100 lines
docker-compose logs --tail=100 api
```

### Manage Services
```bash
# Stop services
docker-compose stop

# Restart services
docker-compose restart

# Remove services
docker-compose down

# Remove with volumes
docker-compose down -v
```

### Execute Commands
```bash
# Access API container
docker-compose exec api bash

# Access PostgreSQL
docker-compose exec postgres psql -U vendorrisk -d VendorRiskDB

# Run migrations
docker-compose exec api dotnet ef database update
```

---

## Health Checks

### API Health
```bash
curl http://localhost:5001/api/health
```

**Expected Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-14T10:00:00Z",
  "version": "1.0.0"
}
```

### Database Health
```bash
docker-compose exec postgres pg_isready -U vendorrisk
```

### Redis Health
```bash
docker-compose exec redis redis-cli ping
```

---

## Volumes

### Data Persistence
- `postgres-data`: PostgreSQL data
- `redis-data`: Redis data
- `./logs`: Application logs (mapped to host)

### Backup Database
```bash
docker-compose exec postgres pg_dump -U vendorrisk VendorRiskDB > backup.sql
```

### Restore Database
```bash
cat backup.sql | docker-compose exec -T postgres psql -U vendorrisk VendorRiskDB
```

---

## Networking

All services run on `vendorrisk-network` bridge network:
- Services can communicate using service names (e.g., `postgres`, `redis`)
- External access via mapped ports

---

## Production Deployment

### 1. Update Environment
```bash
# Edit docker-compose.yml
ASPNETCORE_ENVIRONMENT=Production
```

### 2. Use Secure Passwords
```bash
# Change default passwords
POSTGRES_PASSWORD=<strong-password>
```

### 3. Enable SSL/TLS
```bash
# Update ASPNETCORE_URLS
ASPNETCORE_URLS=https://+:443;http://+:80
```

### 4. Configure Logging
```bash
# Mount log volume
volumes:
  - ./logs:/app/logs:rw
```

### 5. Resource Limits
```yaml
services:
  api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 1G
        reservations:
          cpus: '1'
          memory: 512M
```

---

## Troubleshooting

### API Not Starting
```bash
# Check logs
docker-compose logs api

# Check if database is ready
docker-compose ps
```

### Database Connection Issues
```bash
# Verify database is running
docker-compose ps postgres

# Check connection from API container
docker-compose exec api ping postgres
```

### Port Already in Use
```bash
# Change ports in docker-compose.yml
ports:
  - "5002:8080"  # Change 5001 to 5002
```

### Clear Everything
```bash
# Stop and remove all
docker-compose down -v

# Remove images
docker rmi vendorrisk-api:latest

# Rebuild from scratch
docker-compose up -d --build
```

---

## Multi-Stage Build

The Dockerfile uses multi-stage build for optimization:

1. **Build Stage**: Restore, build application
2. **Publish Stage**: Publish release build
3. **Runtime Stage**: Lightweight runtime image

**Benefits:**
- Smaller final image (~200MB vs ~1GB)
- Faster deployment
- Enhanced security (no build tools in production)

---

## Testing in Docker

### Run Tests in Container
```bash
# Build test image
docker build -t vendorrisk-tests --target build .

# Run tests
docker run --rm vendorrisk-tests dotnet test
```

### Integration Tests with Docker
```bash
# Start dependencies only
docker-compose up -d postgres redis

# Run integration tests on host
dotnet test

# Cleanup
docker-compose down
```

---

## Monitoring

### Container Stats
```bash
docker stats vendorrisk-api vendorrisk-postgres vendorrisk-redis
```

### Disk Usage
```bash
docker system df
```

### Clean Up
```bash
# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Full cleanup
docker system prune -a --volumes
```

---

## CI/CD Integration

### GitHub Actions Example
```yaml
- name: Build Docker Image
  run: docker build -t vendorrisk-api:${{ github.sha }} .

- name: Run Tests
  run: docker-compose up -d && dotnet test

- name: Push to Registry
  run: docker push vendorrisk-api:${{ github.sha }}
```

---

## Support

For issues or questions:
- Check logs: `docker-compose logs -f`
- Verify health: `curl http://localhost:5001/api/health`
- Restart services: `docker-compose restart`
