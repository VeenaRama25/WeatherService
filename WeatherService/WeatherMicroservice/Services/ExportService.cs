using System.Globalization;
using CsvHelper;
using WeatherMicroservice.Repositories.Interfaces;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Services;

public class ExportService : IExportService
{
    private readonly IWeatherRepository _repo;

    public ExportService(IWeatherRepository repo)
    {
        _repo = repo;
    }

    public async Task<byte[]> ExportCsvAsync(string location, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var snapshots = await _repo.GetHistoricalAsync(location, from, to, ct);

        await using var ms = new MemoryStream();
        await using var writer = new StreamWriter(ms);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader(typeof(Models.WeatherSnapshot));
        await csv.NextRecordAsync();

        foreach (var snapshot in snapshots)
        {
            csv.WriteRecord(snapshot);
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync(ct);
        return ms.ToArray();
    }
}