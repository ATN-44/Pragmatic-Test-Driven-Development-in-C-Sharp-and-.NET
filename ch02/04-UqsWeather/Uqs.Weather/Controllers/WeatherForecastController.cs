using AdamTibi.OpenWeather;
using Microsoft.AspNetCore.Mvc;
using Uqs.Weather.Wrappers;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const int ForecastDays = 5;
    private readonly IClient _client;
    private readonly INowWrapper _nowWrapper;
    private readonly IRandomWrapper _randomWrapper;
    private readonly ILogger<WeatherForecastController> _logger;

    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IClient client, INowWrapper nowWrapper,
        IRandomWrapper randomWrapper)
    {
        _logger = logger;
        _client = client;
        _nowWrapper = nowWrapper;
        _randomWrapper = randomWrapper;
    }

    [HttpGet("ConvertCToF")]
    public double ConvertCToF(double c)
    {
        var f = c * (9d / 5d) + 32;
        _logger.LogInformation("conversion requested");
        return f;
    }

    [HttpGet("GetRealWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetReal()
    {
        const decimal greenwichLat = 51.4810m;
        const decimal greenwichLon = 0.0052m;
        var res = await _client.OneCallAsync(greenwichLat, greenwichLon,
            new[]
            {
                Excludes.Current, Excludes.Minutely,
                Excludes.Hourly, Excludes.Alerts
            }, Units.Metric);

        var wfs = new WeatherForecast[ForecastDays];
        for (var i = 0; i < wfs.Length; i++)
        {
            var wf = wfs[i] = new WeatherForecast();
            wf.Date = res.Daily[i + 1].Dt;
            var forecastedTemp = res.Daily[i + 1].Temp.Day;
            wf.TemperatureC = (int)Math.Round(forecastedTemp);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }

        return wfs;
    }

    [HttpGet("GetRandomWeatherForecast")]
    public IEnumerable<WeatherForecast> GetRandom()
    {
        var wfs = new WeatherForecast[ForecastDays];
        for (var i = 0; i < wfs.Length; i++)
        {
            var wf = wfs[i] = new WeatherForecast();
            wf.Date = _nowWrapper.Now.AddDays(i + 1);
            wf.TemperatureC = _randomWrapper.Next(-20, 55);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }

        return wfs;
    }

    private static string MapFeelToTemp(int temperatureC)
    {
        if (temperatureC <= 0) return Summaries.First();
        var summariesIndex = temperatureC / 5 + 1;
        if (summariesIndex >= Summaries.Length) return Summaries.Last();
        return Summaries[summariesIndex];
    }
}