using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Controllers;

[ApiController]
[Route("api/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IExportService _service;

    public ExportController(IExportService service) => _service = service;

    /// <summary>Export historical weather data for a location as a CSV file.</summary>
    [HttpGet("csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string location,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct = default)
    {
        if (from >= to)
            return BadRequest("'from' must be before 'to'.");

        var csvBytes = await _service.ExportCsvAsync(location, from, to, ct);
        var filename = $"weather_{location.Replace(" ", "_")}_{from:yyyyMMdd}_{to:yyyyMMdd}.csv";
        return File(csvBytes, "text/csv", filename);
    }
}