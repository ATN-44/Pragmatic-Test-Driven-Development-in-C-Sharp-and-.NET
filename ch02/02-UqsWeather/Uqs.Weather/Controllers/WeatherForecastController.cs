using Microsoft.AspNetCore.Mvc;
using AdamTibi.OpenWeather;

namespace Uqs.Weather.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private const int ForecastDays = 5;
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IConfiguration _config;

    private static readonly string[] Summaries = {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild",
        "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [HttpGet("ConvertCToF")]
    public double ConvertCToF(double c)
    {
        double f = c * (9d / 5d) + 32;
        _logger.LogInformation("conversion requested");
        return f;
    }

    [HttpGet("GetRealWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> GetReal()
    {
        const decimal greenwichLat = 51.4810m;
        const decimal greenwichLon = 0.0052m;
        var apiKey = _config["OpenWeather:Key"];
        var httpClient = new HttpClient();
        var openWeatherClient = new Client(apiKey, httpClient);
        var res = await openWeatherClient.OneCallAsync
            (greenwichLat, greenwichLon, new [] {
                Excludes.Current, Excludes.Minutely,
                Excludes.Hourly, Excludes.Alerts }, Units.Metric);

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
        WeatherForecast[] wfs = new WeatherForecast[ForecastDays];
        for(int i = 0;i < wfs.Length;i++)
        {
            var wf = wfs[i] = new WeatherForecast();
            wf.Date = DateTime.Now.AddDays(i + 1);
            wf.TemperatureC = Random.Shared.Next(-20, 55);
            wf.Summary = MapFeelToTemp(wf.TemperatureC);
        }
        return wfs;
    }

    private static string MapFeelToTemp(int temperatureC)
    {
        // Anything <= 0 is "Freezing"
        if (temperatureC <= 0)
        {
            return Summaries.First();
        }
        // Dividing the temperature into 5 intervals
        int summariesIndex = (temperatureC / 5) + 1;
        // Anything >= 45 is "Scorching"
        if (summariesIndex >= Summaries.Length)
        {
            return Summaries.Last();
        }
        return Summaries[summariesIndex];
    }
}
