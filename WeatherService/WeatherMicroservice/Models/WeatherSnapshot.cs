namespace WeatherMicroservice.Models;

public class WeatherSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double TemperatureCelsius { get; set; }
    public double FeelsLikeCelsius { get; set; }
    public int HumidityPercent { get; set; }
    public double WindSpeedMs { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;  // "OpenWeatherMap" | "DataGovSg"
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
}