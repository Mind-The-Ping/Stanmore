using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Stanmore.Repository;
using Stanmore.Repository.UserRepository;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var insightsConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];

builder.Logging.ClearProviders();

if (builder.Environment.IsProduction())
{
    var resourceBuilder = ResourceBuilder.CreateDefault()
       .AddService(
           serviceName: builder.Environment.ApplicationName,
           serviceVersion: "1.0.0");

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeFormattedMessage = true;
        logging.IncludeScopes = true;
        logging.SetResourceBuilder(resourceBuilder);

        logging.AddAzureMonitorLogExporter(o =>
        {
            o.ConnectionString = insightsConnectionString;
        });
    });

    builder.Services.AddOpenTelemetry()
      .ConfigureResource(rb => rb.AddService(builder.Environment.ApplicationName))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddAzureMonitorTraceExporter(o =>
          {
              o.ConnectionString = insightsConnectionString;
          }))
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddRuntimeInstrumentation()
          .AddAzureMonitorMetricExporter(o =>
          {
              o.ConnectionString = insightsConnectionString;
          }));
}
else
{
    builder.Logging.AddConsole();
}

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

    var client = new MongoClient(options.ConnectionString);
    var database = client.GetDatabase(options.Name);

    return database;
});

builder.Services.AddScoped<IPremiumUserRepository, PremiumUserRepository>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var issuers = builder.Configuration.GetSection("Jwt:Issuers").Get<string[]>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuers = issuers,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => Results.Ok("Service alive"));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");

public partial class Program { }
