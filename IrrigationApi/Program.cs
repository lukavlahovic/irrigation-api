using IrrigationApi.BackgroundServices;
using IrrigationApi.Configurations;
using IrrigationApi.Data;
using IrrigationApi.Handlers;
using IrrigationApi.Routers;
using IrrigationApi.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<IrrigationContext>(options => 
    options
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention()
);

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if(string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
}

builder.Services.AddSingleton<IDbConnectionFactory>(sp =>
    new NpgsqlDataSourceFactory(connectionString));
builder.Services.AddSingleton<ISensorReadingService, SensorReadingService>();

// Add MQTT handlers for topics
// Add more handlers here as needed
builder.Services.AddSingleton<IMqttMessageHandler, SensorReadingHandler>();

builder.Services.AddSingleton<IMqttMessageRouter, MqttMessageRouter>();

builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));
builder.Services.AddHostedService<MqttClientService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();