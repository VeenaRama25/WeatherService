# Weather Microservice

A production-ready REST API built with **C# / ASP.NET Core 8** for querying current, forecast, and historical weather data, managing alert subscriptions, and exporting data as CSV. Integrates with [OpenWeatherMap](https://openweathermap.org/api) and [data.gov.sg](https://data.gov.sg/), persists to PostgreSQL with Redis caching, and ships with JWT auth, rate limiting, Swagger/OpenAPI docs, Docker support, and a GitHub Actions CI/CD pipeline that deploys to Azure.

---

## Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Endpoints](#api-endpoints)
- [Running Tests](#running-tests)
- [Docker](#docker)
- [CI/CD](#cicd)
- [Project Structure](#project-structure)

---

## Features

- **Current weather** — fetch live conditions for any location via OpenWeatherMap
- **5-day forecast** — hourly forecast items with temperature, humidity, wind, and condition
- **Historical data** — query persisted snapshots by location and date range
- **CSV export** — stream historical records as a downloadable `.csv` file
- **Alert subscriptions** — subscribe by email to threshold-based weather alerts (temperature, rain, wind)
- **JWT authentication** — all endpoints are secured with Bearer token auth
- **Redis caching** — TTL-backed response cache to minimise upstream API calls
- **Rate limiting** — 60 requests/minute per client via ASP.NET Core's built-in rate limiter
- **Polly resilience** — automatic retry (exponential back-off) and circuit breaker on all HTTP client calls
- **Swagger / OpenAPI** — interactive docs available at `/swagger` in development
- **Health check** — `/health` endpoint for container and load balancer probes
- **Structured logging** — Serilog with console sink and request logging middleware
- **Auto-migration** — EF Core migrations run on startup

---

## Architecture

```
Client / Swagger UI
        │
        ▼
JWT Auth · Rate Limiter · Error Handling Middleware
        │
        ├── WeatherController   (GET /api/weather/current|forecast|historical)
        ├── AlertController     (POST|DELETE|GET /api/alerts)
        └── ExportController    (GET /api/export/csv)
                │
                ├── WeatherService   ──► OpenWeatherMap API  (Polly retry + circuit breaker)
                ├── AlertService     ──► PostgreSQL (EF Core)
                └── ExportService    ──► PostgreSQL → CsvHelper → byte[]
                        │
                        ├── PostgreSQL  (persistent weather snapshots + alert subscriptions)
                        └── Redis       (TTL cache for current / forecast responses)
```

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 8.0+ |
| PostgreSQL | 14+ (or Docker) |
| Redis | 7+ (or Docker) |
| Docker + Docker Compose | optional, recommended |

---

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/WeatherService.git
cd WeatherService
```

### 2. Start PostgreSQL and Redis

If you don't have them installed locally, use Docker:

```bash
docker run -d --name weatherdb \
  -e POSTGRES_DB=weatherdb \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=postgres \
  -p 5432:5432 postgres:16-alpine

docker run -d --name weatherredis \
  -p 6379:6379 redis:7-alpine
```

### 3. Configure the app

Copy the example settings and fill in your OpenWeatherMap API key:

```bash
cp src/WeatherService/appsettings.json src/WeatherService/appsettings.Development.json
```

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=weatherdb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "OpenWeatherMap": {
    "ApiKey": "YOUR_OWM_API_KEY"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-change-this-in-production"
  }
}
```

Get a free OpenWeatherMap API key at [openweathermap.org/api](https://openweathermap.org/api).

### 4. Run EF Core migrations

```bash
dotnet ef migrations add InitialCreate --project src/WeatherService
dotnet ef database update --project src/WeatherService
```

### 5. Run the API

```bash
dotnet run --project src/WeatherService
```

Open Swagger UI at: **http://localhost:5000/swagger**

---

## Configuration

All settings live in `appsettings.json`. Override per environment via `appsettings.{Environment}.json` or environment variables.

| Key | Description | Default |
|---|---|---|
| `ConnectionStrings:DefaultConnection` | PostgreSQL connection string | `Host=localhost;...` |
| `ConnectionStrings:Redis` | Redis connection string | `localhost:6379` |
| `Jwt:Key` | 256-bit signing secret | — |
| `Jwt:Issuer` | JWT issuer claim | `WeatherMicroservice` |
| `Jwt:Audience` | JWT audience claim | `WeatherMicroserviceClients` |
| `Jwt:ExpiryMinutes` | Token lifetime in minutes | `60` |
| `OpenWeatherMap:ApiKey` | OWM API key | — |
| `OpenWeatherMap:Units` | Unit system (`metric`/`imperial`) | `metric` |
| `Cache:CurrentWeatherTtlSeconds` | Redis TTL for current weather | `300` |
| `Cache:ForecastTtlSeconds` | Redis TTL for forecasts | `900` |
| `Cache:HistoricalTtlSeconds` | Redis TTL for historical queries | `3600` |

---

## API Endpoints

All endpoints require a `Authorization: Bearer {token}` header.

### Weather

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/weather/current?location={city}` | Current weather for a location |
| `GET` | `/api/weather/forecast?location={city}&days={1-7}` | N-day forecast (default 5) |
| `GET` | `/api/weather/historical?location={city}&from={date}&to={date}` | Historical snapshots |

### Alerts

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/alerts/subscribe` | Subscribe to weather alerts |
| `DELETE` | `/api/alerts/{id}` | Unsubscribe by subscription ID |
| `GET` | `/api/alerts?email={email}` | List active subscriptions for an email |

**Subscribe request body:**

```json
{
  "email": "you@example.com",
  "location": "Singapore",
  "maxTemperatureCelsius": 35,
  "minTemperatureCelsius": 20,
  "alertOnRain": true,
  "alertOnHighWind": true,
  "windThresholdMs": 10.0
}
```

### Export

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/export/csv?location={city}&from={date}&to={date}` | Download historical data as CSV |

### Health

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/health` | Liveness probe — returns `200 Healthy` |

---

## Running Tests

```bash
dotnet test
```

Tests use xUnit, Moq, and FluentAssertions. The test project targets the main project via `<ProjectReference>` — no running infrastructure needed for unit tests.

---

## Docker

### Run everything with Docker Compose

```bash
# Set your OpenWeatherMap API key
export OWM_API_KEY=your_key_here   # Windows: set OWM_API_KEY=your_key_here

docker compose up --build
```

This starts three containers: the API on port `8080`, PostgreSQL, and Redis. Migrations run automatically on startup.

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Health | http://localhost:8080/health |

### Build the image only

```bash
docker build -t weather-microservice .
```

---

## CI/CD

The GitHub Actions workflow at `.github/workflows/ci-cd.yml` runs on every push and pull request to `main`:

1. **Build** — `dotnet build -c Release`
2. **Test** — `dotnet test` against real PostgreSQL and Redis service containers
3. **Publish** — `dotnet publish -c Release`
4. **Deploy** — pushes to Azure App Service on merges to `main` (requires `AZURE_WEBAPP_PUBLISH_PROFILE` secret)

To enable deployment, add your Azure publish profile as a repository secret named `AZURE_WEBAPP_PUBLISH_PROFILE`, and update `AZURE_WEBAPP_NAME` in the workflow file to match your Azure App Service name.

---

## Project Structure

```
WeatherService/
├── src/
│   └── WeatherService/
│       ├── Controllers/          # WeatherController, AlertController, ExportController
│       ├── Data/                 # AppDbContext + EF Core configuration
│       ├── Middleware/           # Global error handling middleware
│       ├── Models/               # WeatherSnapshot, AlertSubscription, DTOs
│       ├── Repositories/        # IWeatherRepository + EF Core implementation
│       ├── Services/             # IWeatherService, IAlertService, IExportService + implementations
│       ├── Program.cs            # App bootstrap, DI, middleware pipeline
│       └── appsettings.json      # Default configuration
├── tests/
│   └── WeatherMicroservice.Tests/
│       ├── WeatherServiceTests.cs
│       └── WeatherControllerTests.cs
├── .github/workflows/ci-cd.yml   # GitHub Actions CI/CD pipeline
├── Dockerfile
├── docker-compose.yml
└── WeatherMicroservice.sln
```

---

## Security Notes

- Never commit real values for `Jwt:Key` or `OpenWeatherMap:ApiKey` — use environment variables or Azure Key Vault in production
- The default `appsettings.json` contains placeholder values only
- JWT tokens expire after 60 minutes by default; adjust `Jwt:ExpiryMinutes` as needed
- Rate limiting is set to 60 requests/minute — tune `PermitLimit` and `Window` in `Program.cs` for your use case
