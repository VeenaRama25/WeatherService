using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Controllers;

[ApiController]
[Route("api/weather")]
[Authorize]
[EnableRateLimiting("weather-api")]
[Produces("application/json")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _service;

    public WeatherController(IWeatherService service) => _service = service;

    /// <summary>Get current weather for a location.</summary>
    [HttpGet("current")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrent([FromQuery] string location, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest("Location is required.");

        var result = await _service.GetCurrentAsync(location, ct);
        return Ok(result);
    }

    /// <summary>Get weather forecast for a location.</summary>
    [HttpGet("forecast")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForecast(
        [FromQuery] string location,
        [FromQuery] int days = 5,
        CancellationToken ct = default)
    {
        if (days is < 1 or > 7)
            return BadRequest("Days must be between 1 and 7.");

        var result = await _service.GetForecastAsync(location, days, ct);
        return Ok(result);
    }

    /// <summary>Get historical weather data for a location and date range.</summary>
    [HttpGet("historical")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistorical(
        [FromQuery] string location,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken ct = default)
    {
        if (from >= to)
            return BadRequest("'from' must be before 'to'.");

        var result = await _service.GetHistoricalAsync(location, from, to, ct);
        return Ok(result);
    }
}