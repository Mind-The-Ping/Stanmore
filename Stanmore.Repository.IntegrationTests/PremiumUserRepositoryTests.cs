using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Stanmore.Repository.UserRepository;

namespace Stanmore.Repository.IntegrationTests;

public class PremiumUserRepositoryTests
{
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IMongoCollection<PremiumUser> _premiumUserCollection;

    private readonly ILogger<PremiumUserRepository> _logger;
    private readonly IOptions<DatabaseOptions> _databaseOptions;
    private readonly PremiumUserRepository _premiumUserRepository;

    private readonly string _databaseName = $"testdb_{Guid.NewGuid():N}";

    public PremiumUserRepositoryTests()
    {
        var client = new MongoClient("mongodb://localhost:27017");
        _mongoDatabase = client.GetDatabase(_databaseName);

        var databaseOptions = new DatabaseOptions()
        {
            Name = "Stanmore",
            Collection = "PremiumUsers",
            ConnectionString = "mongodb://localhost:27017"
        };

        _logger = NSubstitute.Substitute.For<ILogger<PremiumUserRepository>>();
        _databaseOptions = Options.Create(databaseOptions);

        _premiumUserRepository = new PremiumUserRepository(_mongoDatabase, _databaseOptions, _logger);
        _premiumUserCollection = _mongoDatabase.GetCollection<PremiumUser>(databaseOptions.Collection);
    }
    private async Task InitializeAsync()
    {
        await _mongoDatabase.Client.DropDatabaseAsync(_databaseName);
    }

    [Fact]
    public void PremiumUserRepository_Ctor_NullDatabase_Throws_ArguementNullException()
    {
        var exception = Assert
            .Throws<ArgumentNullException>(() => new PremiumUserRepository(
                null!,
                _databaseOptions,
                _logger));

        exception.Message.Should().Be("Value cannot be null. (Parameter 'mongoDatabase')");
    }

    [Fact]
    public void PremiumUserRepository_Ctor_NullDatabaseOptions_Throws_ArguementNullException()
    {
        var exception = Assert
            .Throws<ArgumentNullException>(() => new PremiumUserRepository(
                _mongoDatabase,
                null!,
                _logger));

        exception.Message.Should().Be("Value cannot be null. (Parameter 'options')");
    }

    [Fact]
    public void PremiumUserRepository_Ctor_NullLogger_Throws_ArguementNullException()
    {
        var exception = Assert
            .Throws<ArgumentNullException>(() => new PremiumUserRepository(
                _mongoDatabase,
                _databaseOptions,
                null!));

        exception.Message.Should().Be("Value cannot be null. (Parameter 'logger')");
    }

    [Fact]
    public async Task PremiumUserRepository_AddPremiumUserAsync_Successful()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser 
        { 
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        var result = await _premiumUserRepository.AddPremiumUserAsync(premiumUser);

        result.IsSuccess.Should().BeTrue();

        var record = await _premiumUserCollection
           .Find(x => x.UserId == premiumUser.UserId)
           .SingleOrDefaultAsync();

        record.UserId.Should().Be(premiumUser.UserId);
        record.PremiumExpiresAt.Should().BeCloseTo(premiumUser.PremiumExpiresAt, TimeSpan.FromSeconds(5));
        record.CreatedAt.Should().BeCloseTo(premiumUser.CreatedAt, TimeSpan.FromSeconds(5));
        record.UpdatedAt.Should().BeCloseTo(premiumUser.UpdatedAt, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PremiumUserRepository_GetPremiumUserAsync_Successful()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _premiumUserRepository.AddPremiumUserAsync(premiumUser);

        var result = await _premiumUserRepository.GetPremiumUserAsync(premiumUser.UserId);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(premiumUser.UserId);
        result.Value.PremiumExpiresAt.Should().BeCloseTo(premiumUser.PremiumExpiresAt, TimeSpan.FromSeconds(5));
        result.Value.CreatedAt.Should().BeCloseTo(premiumUser.CreatedAt, TimeSpan.FromSeconds(5));
        result.Value.UpdatedAt.Should().BeCloseTo(premiumUser.UpdatedAt, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PremiumUserRepository_GetPremiumUserAsync_No_User_Fails()
    {
        await InitializeAsync();

        var userId = Guid.NewGuid();

        var result = await _premiumUserRepository.GetPremiumUserAsync(userId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"User {userId} does not exist.");
    }

    [Fact]
    public async Task PremiumUserRepository_DeletePreiumUserAsync_Successful()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _premiumUserRepository.AddPremiumUserAsync(premiumUser);

        var result = await _premiumUserRepository.DeletePreiumUserAsync(premiumUser.UserId);

        result.IsSuccess.Should().BeTrue();

        var record = await _premiumUserCollection
           .Find(x => x.UserId == premiumUser.UserId)
           .SingleOrDefaultAsync();

        record.Should().BeNull();
    }

    [Fact]
    public async Task PremiumUserRepository_DeletePreiumUserAsync_No_User_Fails()
    {
        await InitializeAsync();

        var userId = Guid.NewGuid();

        var result = await _premiumUserRepository.DeletePreiumUserAsync(userId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Premium user {userId} was not found.");
    }

    [Fact]
    public async Task PremiumUserRepository_IsUserPremiumAsync_Successful()
    {
        await InitializeAsync();

        var premiumUser = new PremiumUser
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _premiumUserRepository.AddPremiumUserAsync(premiumUser);

        var result = await _premiumUserRepository.IsUserPremiumAsync(premiumUser.UserId);
        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task PremiumUserRepository_IsUserPremiumAsync_No_User_False()
    {

        await InitializeAsync();

        var userId = Guid.NewGuid();

        var result = await _premiumUserRepository.IsUserPremiumAsync(userId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task PremiumUserRepository_IsUserPremiumAsync_EpiryDate_Past_Now_False()
    {

        await InitializeAsync();

        var premiumUser = new PremiumUser
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(-5),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _premiumUserRepository.AddPremiumUserAsync(premiumUser);

        var result = await _premiumUserRepository.IsUserPremiumAsync(premiumUser.UserId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task PremiumUserRepository_UpdatePremiumUserExpiryTimeAsync_Successful()
    {
        await InitializeAsync();

        var newExpirationDate = DateTime.UtcNow.AddDays(30);

        var premiumUser = new PremiumUser
        {
            UserId = Guid.NewGuid(),
            PremiumExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-30),
        };

        await _premiumUserRepository.AddPremiumUserAsync(premiumUser);

        var result = await _premiumUserRepository
            .UpdatePremiumUserExpiryTimeAsync(premiumUser.UserId, newExpirationDate);

        result.IsSuccess.Should().BeTrue();

        var record = await _premiumUserCollection
           .Find(x => x.UserId == premiumUser.UserId)
           .SingleOrDefaultAsync();

        record.PremiumExpiresAt.Should().BeCloseTo(newExpirationDate, TimeSpan.FromSeconds(5));
        record.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task PremiumUserRepository_UpdatePremiumUserExpiryTimeAsync_No_User_Fails()
    {
        await InitializeAsync();

        var userId = Guid.NewGuid();
        var newExpirationDate = DateTime.UtcNow.AddDays(30);

        var result = await _premiumUserRepository
         .UpdatePremiumUserExpiryTimeAsync(userId, newExpirationDate);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"{userId} does not exist.");
    }
}
