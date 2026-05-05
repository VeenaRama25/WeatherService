namespace WeatherMicroservice.Models;

public class AlertSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double? MaxTemperatureCelsius { get; set; }
    public double? MinTemperatureCelsius { get; set; }
    public bool AlertOnRain { get; set; }
    public bool AlertOnHighWind { get; set; }
    public double? WindThresholdMs { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}