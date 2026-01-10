using SkyLab.Backend.Hubs;
using SkyLab.Backend.Workers;
using SkyLab.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<McpClientSdkService>();
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddSingleton<FlightStateService>();
builder.Services.AddHostedService<FlightSimulationWorker>();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173") // Vite dev and preview ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();



app.UseCors();



app.MapHub<FlightHub>("/flighthub");

app.MapControllers();



app.Run();
