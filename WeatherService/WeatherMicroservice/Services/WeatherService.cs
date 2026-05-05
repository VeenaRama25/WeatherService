using System.Text.Json;
using StackExchange.Redis;
using WeatherMicroservice.Models;
using WeatherMicroservice.Models.DTOs;
using WeatherMicroservice.Repositories.Interfaces;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Services;

public class WeatherService : IWeatherService
{
    private readonly IHttpClientFactory _http;
    private readonly IWeatherRepository _repo;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IHttpClientFactory http,
        IWeatherRepository repo,
        IConnectionMultiplexer redis,
        IConfiguration config,
        ILogger<WeatherService> logger)
    {
        _http = http;
        _repo = repo;
        _redis = redis;
        _config = config;
        _logger = logger;
    }

    public async Task<WeatherResponse> GetCurrentAsync(string location, CancellationToken ct = default)
    {
        var cacheKey = $"weather:current:{location.ToLowerInvariant()}";
        var db = _redis.GetDatabase();
        var cached = await db.StringGetAsync(cacheKey);

        if (cached.HasValue)
        {
            _logger.LogInformation("Cache hit for current weather: {Location}", location);
            return JsonSerializer.Deserialize<WeatherResponse>(cached!)!;
        }

        var snapshot = await FetchFromOpenWeatherMapAsync(location, ct);
        await _repo.SaveSnapshotAsync(snapshot, ct);

        var response = MapToResponse(snapshot);
        var ttl = TimeSpan.FromSeconds(_config.GetValue<int>("Cache:CurrentWeatherTtlSeconds"));
        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(response), ttl);

        return response;
    }

    public async Task<ForecastResponse> GetForecastAsync(string location, int days = 5, CancellationToken ct = default)
    {
        var cacheKey = $"weather:forecast:{location.ToLowerInvariant()}:{days}";
        var db = _redis.GetDatabase();
        var cached = await db.StringGetAsync(cacheKey);

        if (cached.HasValue)
            return JsonSerializer.Deserialize<ForecastResponse>(cached!)!;

        var client = _http.CreateClient("OpenWeatherMap");
        var apiKey = _config["OpenWeatherMap:ApiKey"];
        var url = $"forecast?q={Uri.EscapeDataString(location)}&appid={apiKey}&units=metric&cnt={days * 8}";

        var json = await client.GetStringAsync(url, ct);
        using var doc = JsonDocument.Parse(json);

        var items = doc.RootElement
            .GetProperty("list")
            .EnumerateArray()
            .Select(item => new ForecastItem(
                DateTime.Parse(item.GetProperty("dt_txt").GetString()!),
                item.GetProperty("main").GetProperty("temp").GetDouble(),
                item.GetProperty("main").GetProperty("feels_like").GetDouble(),
                item.GetProperty("main").GetProperty("humidity").GetInt32(),
                item.GetProperty("wind").GetProperty("speed").GetDouble(),
                item.GetProperty("weather")[0].GetProperty("description").GetString()!
            ));

        var result = new ForecastResponse(location, items);
        var ttl = TimeSpan.FromSeconds(_config.GetValue<int>("Cache:ForecastTtlSeconds"));
        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(result), ttl);

        return result;
    }

    public async Task<IEnumerable<WeatherResponse>> GetHistoricalAsync(
        string location, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var snapshots = await _repo.GetHistoricalAsync(location, from, to, ct);
        return snapshots.Select(MapToResponse);
    }

    private async Task<WeatherSnapshot> FetchFromOpenWeatherMapAsync(string location, CancellationToken ct)
    {
        var client = _http.CreateClient("OpenWeatherMap");
        var apiKey = _config["OpenWeatherMap:ApiKey"];
        var url = $"weather?q={Uri.EscapeDataString(location)}&appid={apiKey}&units=metric";

        var json = await client.GetStringAsync(url, ct);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        return new WeatherSnapshot
        {
            Location = location,
            Latitude = root.GetProperty("coord").GetProperty("lat").GetDouble(),
            Longitude = root.GetProperty("coord").GetProperty("lon").GetDouble(),
            TemperatureCelsius = root.GetProperty("main").GetProperty("temp").GetDouble(),
            FeelsLikeCelsius = root.GetProperty("main").GetProperty("feels_like").GetDouble(),
            HumidityPercent = root.GetProperty("main").GetProperty("humidity").GetInt32(),
            WindSpeedMs = root.GetProperty("wind").GetProperty("speed").GetDouble(),
            Condition = root.GetProperty("weather")[0].GetProperty("description").GetString()!,
            Source = "OpenWeatherMap",
            RecordedAt = DateTime.UtcNow
        };
    }

    private static WeatherResponse MapToResponse(WeatherSnapshot s) =>
        new(s.Location, s.Latitude, s.Longitude, s.TemperatureCelsius,
            s.FeelsLikeCelsius, s.HumidityPercent, s.WindSpeedMs, s.Condition,
            s.Source, s.RecordedAt);
}