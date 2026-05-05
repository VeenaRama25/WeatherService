using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherMicroservice.Models;
using WeatherMicroservice.Repositories.Interfaces;
using WeatherMicroservice.Services;

namespace WeatherMicroservice.Tests;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherRepository> _repoMock = new();
    private readonly Mock<IHttpClientFactory> _httpMock = new();
    private readonly Mock<StackExchange.Redis.IConnectionMultiplexer> _redisMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<ILogger<WeatherService>> _loggerMock = new();

    [Fact]
    public async Task GetHistoricalAsync_ReturnsSnapshotsFromRepository()
    {
        var from = DateTime.UtcNow.AddDays(-7);
        var to = DateTime.UtcNow;
        var snapshots = new List<WeatherSnapshot>
        {
            new() { Location = "Singapore", TemperatureCelsius = 30, RecordedAt = from.AddDays(1) },
            new() { Location = "Singapore", TemperatureCelsius = 31, RecordedAt = from.AddDays(2) }
        };

        _repoMock
            .Setup(r => r.GetHistoricalAsync("Singapore", from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshots);

        var db = new Mock<StackExchange.Redis.IDatabase>();
        db.Setup(d => d.StringGetAsync(It.IsAny<StackExchange.Redis.RedisKey>(), It.IsAny<StackExchange.Redis.CommandFlags>()))
          .ReturnsAsync(StackExchange.Redis.RedisValue.Null);
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);

        var service = new WeatherService(_httpMock.Object, _repoMock.Object,
            _redisMock.Object, _configMock.Object, _loggerMock.Object);

        var results = await service.GetHistoricalAsync("Singapore", from, to);

        results.Should().HaveCount(2);
        results.First().TemperatureCelsius.Should().Be(30);
    }
}