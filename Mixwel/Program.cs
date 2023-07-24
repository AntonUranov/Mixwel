using HealthChecks.UI.Client;
using Mixwel;
using Mixwel.Infrastructure.Health;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Add services to the container.
builder.Services.RegisterDependencies();
builder.Services.ConfigureRedis(() => 
    builder.Configuration.GetConnectionString("Redis"));

builder.Services.ConfigureHttpClients();

builder.Services.AddControllers();
builder.Services.AddApiVersioning();
builder.Services.AddHealthChecks()
    .AddCheck<RedisHealth>("RedisHealth")
    .AddCheck<ProvidersHealth>("ProvidersHealth");
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/api/ping", 
    new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions 
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

await app.SetupRedis();

app.Run();
