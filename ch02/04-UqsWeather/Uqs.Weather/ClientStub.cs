using AdamTibi.OpenWeather;

namespace Uqs.Weather;

public class ClientStub : IClient
{
    public Task<OneCallResponse> OneCallAsync(decimal latitude, decimal longitude, IEnumerable<Excludes> excludes, Units unit)
    {
        const int days = 7;
        var res = new OneCallResponse { Daily = new Daily[days] };
        var now = DateTime.Now;
        for (var i = 0; i < days; i++)
        {
            res.Daily[i] = new Daily
            {
                Dt = now.AddDays(i),
                Temp = new Temp
                {
                    Day = Random.Shared.Next(-20, 55)
                }
            };
        }

        return Task.FromResult(res);
    }
}