using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WeatherMicroservice.Controllers;
using WeatherMicroservice.Models.DTOs;
using WeatherMicroservice.Services.Interfaces;

namespace WeatherMicroservice.Tests;

public class WeatherControllerTests
{
    private readonly Mock<IWeatherService> _serviceMock = new();

    [Fact]
    public async Task GetCurrent_ReturnsOk_WithValidLocation()
    {
        var expected = new WeatherResponse(
            "Singapore", 1.3521, 103.8198, 30.5, 34.0,
            85, 5.2, "light rain", "OpenWeatherMap", DateTime.UtcNow);

        _serviceMock
            .Setup(s => s.GetCurrentAsync("Singapore", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new WeatherController(_serviceMock.Object);
        var result = await controller.GetCurrent("Singapore", CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetCurrent_ReturnsBadRequest_WhenLocationIsEmpty()
    {
        var controller = new WeatherController(_serviceMock.Object);
        var result = await controller.GetCurrent("", CancellationToken.None);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetForecast_ReturnsBadRequest_WhenDaysOutOfRange()
    {
        var controller = new WeatherController(_serviceMock.Object);
        var result = await controller.GetForecast("Singapore", days: 10);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}