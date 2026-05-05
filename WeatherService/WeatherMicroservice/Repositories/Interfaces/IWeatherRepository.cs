using WeatherMicroservice.Models;

namespace WeatherMicroservice.Repositories.Interfaces;

public interface IWeatherRepository
{
    Task SaveSnapshotAsync(WeatherSnapshot snapshot, CancellationToken ct = default);
    Task<IEnumerable<WeatherSnapshot>> GetHistoricalAsync(string location, DateTime from, DateTime to, CancellationToken ct = default);
}