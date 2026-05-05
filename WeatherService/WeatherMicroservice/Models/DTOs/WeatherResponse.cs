namespace WeatherMicroservice.Models.DTOs;

public record WeatherResponse(
    string Location,
    double Latitude,
    double Longitude,
    double TemperatureCelsius,
    double FeelsLikeCelsius,
    int HumidityPercent,
    double WindSpeedMs,
    string Condition,
    string Source,
    DateTime RecordedAt
);

public record ForecastItem(
    DateTime DateTime,
    double TemperatureCelsius,
    double FeelsLikeCelsius,
    int HumidityPercent,
    double WindSpeedMs,
    string Condition
);

public record ForecastResponse(string Location, IEnumerable<ForecastItem> Items);

public record AlertRequest(
    string Email,
    string Location,
    double? MaxTemperatureCelsius,
    double? MinTemperatureCelsius,
    bool AlertOnRain,
    bool AlertOnHighWind,
    double? WindThresholdMs
);