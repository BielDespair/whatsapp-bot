var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/", () =>
{
    return Results.Ok("Hello, World!");
})
.WithName("GetWeatherForecast");
app.MapPost("/webhook", async (HttpRequest req) =>
{
    return Results.Ok();
});


app.Run();