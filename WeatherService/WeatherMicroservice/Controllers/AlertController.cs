using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeatherMicroservice.Models.DTOs;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Controllers;

[ApiController]
[Route("api/alerts")]
[Authorize]
[Produces("application/json")]
public class AlertController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertController(IAlertService service) => _service = service;

    /// <summary>Subscribe to weather alerts for a location.</summary>
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Subscribe([FromBody] AlertRequest request, CancellationToken ct)
    {
        var result = await _service.SubscribeAsync(request, ct);
        return CreatedAtAction(nameof(GetSubscriptions), new { email = request.Email }, result);
    }

    /// <summary>Unsubscribe from a weather alert.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unsubscribe(Guid id, CancellationToken ct)
    {
        var deleted = await _service.UnsubscribeAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }

    /// <summary>Get all active alert subscriptions for an email address.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptions([FromQuery] string email, CancellationToken ct)
    {
        var subs = await _service.GetSubscriptionsAsync(email, ct);
        return Ok(subs);
    }
}