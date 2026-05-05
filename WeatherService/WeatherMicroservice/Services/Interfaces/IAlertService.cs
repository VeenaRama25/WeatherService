using WeatherMicroservice.Models;
using WeatherMicroservice.Models.DTOs;

namespace WeatherMicroservice.Services.Interfaces;

public interface IAlertService
{
    Task<AlertSubscription> SubscribeAsync(AlertRequest request, CancellationToken ct = default);
    Task<bool> UnsubscribeAsync(Guid subscriptionId, CancellationToken ct = default);
    Task<IEnumerable<AlertSubscription>> GetSubscriptionsAsync(string email, CancellationToken ct = default);
}
