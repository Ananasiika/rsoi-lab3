using GatewayService;
using GatewayService.HttpClients;
using GatewayService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Flight Booking System Gateway",
        Version = "v1",
        Description = "Gateway API for Flight Booking System"
    });
});

builder.Services.AddSingleton<CircuitBreaker>();
builder.Services.AddSingleton<IRetryQueue, RetryQueue>();
builder.Services.AddHostedService<RetryQueue>(provider =>
    (RetryQueue)provider.GetRequiredService<IRetryQueue>());
// Register HTTP clients
builder.Services.AddHttpClient<IFlightClient, FlightClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:FlightService"] ?? "http://flight_service:8060");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IBonusClient, BonusClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:BonusService"] ?? "http://bonus_service:8050");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ITicketClient, TicketClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:TicketService"] ?? "http://ticket_service:8070");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Register services
builder.Services.AddScoped<IGatewayService, GatewayService.Services.GatewayService>();

// Add logging
builder.Services.AddLogging();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Flight Booking System Gateway v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// Health check endpoint
app.MapGet("/manage/health", () => Results.Ok(new { status = "Healthy", service = "Gateway" }));

// Добавьте это для отладки
app.MapGet("/", () => "Gateway Service is running! Go to /swagger for API documentation");

Console.WriteLine("Application is starting...");
Console.WriteLine("Swagger will be available at: http://localhost:8080/swagger");

app.Run();