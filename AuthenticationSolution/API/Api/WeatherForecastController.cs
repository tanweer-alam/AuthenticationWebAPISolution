using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly List<string> Summaries = new List<string>
        {
            "Freezing", "Bracing", "Chilly"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        //[Authorize(Policy = "AdminPolicy")]
        //Role-based Authorization Roles = "User, Admin"
        //user must contain same role, no heirchy
        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, Summaries.Count).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[index-1]
            })
            .ToArray();
        }
        [HttpPost]
        public ActionResult<CreateWeatherRequest> Post(CreateWeatherRequest request)
        {
            Summaries.Add(request.Summary);
            return Ok(request);
        }
    }
}
