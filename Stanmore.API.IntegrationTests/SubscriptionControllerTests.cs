using FluentAssertions;
using MongoDB.Driver;
using Stanmore.Repository;
using System.Net;
using System.Net.Http.Headers;

namespace Stanmore.API.IntegrationTests;

public class SubscriptionControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    private readonly HttpClient _client;
    private readonly HttpClient _unauthorizedClient;
    private readonly Guid _id = Guid.NewGuid();
    private readonly IMongoCollection<PremiumUser> _premiumUserCollection;

    public SubscriptionControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        var token = _factory.GenerateTestJwt(_id);
        _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        _unauthorizedClient = factory.CreateClient();

        _premiumUserCollection = factory.Database.GetCollection<PremiumUser>("PremiumUsers");
    }

    private async Task InitializeAsync() {
        await _factory.ResetDatabaseAsync();
    }

    [Fact]
    public async Task SubscriptionController__GetPremiumUser__Successful()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser()
        {
            UserId = _id,
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _premiumUserCollection.InsertOneAsync(premiumUser);

        var response = await _client.GetAsync("api/subscription/premiumUser");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SubscriptionController__GetPremiumUser__UnAuthorized__Fails()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser()
        {
            UserId = _id,
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _premiumUserCollection.InsertOneAsync(premiumUser);

        var response = await _unauthorizedClient.GetAsync("api/subscription/premiumUser");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SubScriptionController__GetPremiumUserById__Successful()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser()
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _premiumUserCollection.InsertOneAsync(premiumUser);

        var response = await _client.GetAsync($"api/subscription/premiumUserById?userId={premiumUser.UserId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SubScriptionController__GetPremiumUserById_UnAuthorized_Fails()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser()
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _premiumUserCollection.InsertOneAsync(premiumUser);

        var response = await _unauthorizedClient.GetAsync($"api/subscription/premiumUserById?userId={premiumUser.UserId}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
