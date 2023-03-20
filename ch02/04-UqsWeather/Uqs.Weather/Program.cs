using AdamTibi.OpenWeather;
using Uqs.Weather;
using Uqs.Weather.Wrappers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IClient>(_ => {
    var isLoad = bool.Parse(builder.Configuration["LoadTest:IsActive"]);
    if (isLoad) { return new ClientStub(); }

    var apiKey = builder.Configuration["OpenWeather:Key"];
    var httpClient = new HttpClient();
    return new Client(apiKey, httpClient); });

builder.Services.AddSingleton<INowWrapper>(_ => new NowWrapper());
builder.Services.AddTransient<IRandomWrapper>(_ => new RandomWrapper());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();