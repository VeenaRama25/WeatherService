using WeatherMicroservice.Models.DTOs;

namespace WeatherMicroservice.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherResponse> GetCurrentAsync(string location, CancellationToken ct = default);
    Task<ForecastResponse> GetForecastAsync(string location, int days = 5, CancellationToken ct = default);
    Task<IEnumerable<WeatherResponse>> GetHistoricalAsync(string location, DateTime from, DateTime to, CancellationToken ct = default);
}