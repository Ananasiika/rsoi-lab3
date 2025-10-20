using Microsoft.EntityFrameworkCore;
using TicketService.Database;
using TicketService.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database configuration
builder.Services.AddDbContext<TicketDatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")),
                ServiceLifetime.Transient, ServiceLifetime.Transient);

// Services
builder.Services.AddScoped<ITicketService, TicketService.Services.TicketService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

app.UseAuthorization();
app.MapControllers();
app.MapGet("/manage/health", () => Results.Ok(new { status = "Healthy", service = "ticket" }));
app.Run();