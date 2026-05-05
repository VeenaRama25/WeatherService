namespace WeatherMicroservice.Services.Interfaces;

public interface IExportService
{
    Task<byte[]> ExportCsvAsync(string location, DateTime from, DateTime to, CancellationToken ct = default);
}