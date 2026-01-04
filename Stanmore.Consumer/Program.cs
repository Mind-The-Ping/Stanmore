using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Stanmore.Consumer;
using Stanmore.Consumer.SubscriptionHandler;
using Stanmore.Repository;
using Stanmore.Repository.UserRepository;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection("Database"));

builder.Services.Configure<RevenueCatOptions>(
    builder.Configuration.GetSection("RevenueCat"));

builder.Services.AddSingleton(sp =>
{
    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

    var client = new MongoClient(options.ConnectionString);
    var database = client.GetDatabase(options.Name);

    return database;
});

builder.Services.AddScoped<IPremiumUserRepository, PremiumUserRepository>();
builder.Services.AddScoped<ISubscriptionEventHandler, SubscriptionEventHandler>();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Logging
    .ClearProviders()
    .AddConsole()
    .AddApplicationInsights();

builder.Build().Run();
