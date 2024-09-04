using Microsoft.AspNetCore.Mvc;

namespace RtcSignaling.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    
    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "Rooms")]
    public IEnumerable<Room.Room> Get()
    {
        return new List<Room.Room>();
    }
}