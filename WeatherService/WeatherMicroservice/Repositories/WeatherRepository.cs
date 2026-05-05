using Microsoft.EntityFrameworkCore;
using WeatherMicroservice.Data;
using WeatherMicroservice.Models;
using WeatherMicroservice.Repositories.Interfaces;

namespace WeatherMicroservice.Repositories;

public class WeatherRepository : IWeatherRepository
{
    private readonly AppDbContext _db;

    public WeatherRepository(AppDbContext db) => _db = db;

    public async Task SaveSnapshotAsync(WeatherSnapshot snapshot, CancellationToken ct = default)
    {
        _db.WeatherSnapshots.Add(snapshot);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<WeatherSnapshot>> GetHistoricalAsync(
        string location, DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await _db.WeatherSnapshots
            .Where(s => s.Location.ToLower() == location.ToLower()
                     && s.RecordedAt >= from
                     && s.RecordedAt <= to)
            .OrderBy(s => s.RecordedAt)
            .ToListAsync(ct);
    }
}