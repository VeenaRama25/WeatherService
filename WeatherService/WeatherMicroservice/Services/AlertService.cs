using Microsoft.EntityFrameworkCore;
using WeatherMicroservice.Data;
using WeatherMicroservice.Models;
using WeatherMicroservice.Models.DTOs;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _db;
    private readonly ILogger<AlertService> _logger;

    public AlertService(AppDbContext db, ILogger<AlertService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AlertSubscription> SubscribeAsync(AlertRequest request, CancellationToken ct = default)
    {
        var subscription = new AlertSubscription
        {
            Email = request.Email,
            Location = request.Location,
            MaxTemperatureCelsius = request.MaxTemperatureCelsius,
            MinTemperatureCelsius = request.MinTemperatureCelsius,
            AlertOnRain = request.AlertOnRain,
            AlertOnHighWind = request.AlertOnHighWind,
            WindThresholdMs = request.WindThresholdMs
        };

        _db.AlertSubscriptions.Add(subscription);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Alert subscription created for {Email} at {Location}", request.Email, request.Location);
        return subscription;
    }

    public async Task<bool> UnsubscribeAsync(Guid subscriptionId, CancellationToken ct = default)
    {
        var sub = await _db.AlertSubscriptions.FindAsync(new object[] { subscriptionId }, ct);
        if (sub is null) return false;

        sub.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IEnumerable<AlertSubscription>> GetSubscriptionsAsync(string email, CancellationToken ct = default)
    {
        return await _db.AlertSubscriptions
            .Where(s => s.Email == email && s.IsActive)
            .ToListAsync(ct);
    }
}