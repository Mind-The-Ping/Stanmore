using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Stanmore.Repository.UserRepository;

public class PremiumUserRepository : IPremiumUserRepository
{
    private readonly ILogger<PremiumUserRepository> _logger;
    private readonly IMongoCollection<PremiumUser> _premiumUserCollection;

    public PremiumUserRepository(
        IMongoDatabase mongoDatabase,
        IOptions<DatabaseOptions> options,
        ILogger<PremiumUserRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(mongoDatabase, nameof(mongoDatabase));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _premiumUserCollection = mongoDatabase.GetCollection<PremiumUser>(options.Value.Collection);
    }

    public async Task<Result<PremiumUser>> GetPremiumUserAsync(Guid userId)
    {
        var user = await _premiumUserCollection.
            Find(x => x.UserId == userId)
            .SingleOrDefaultAsync();

        if (user == null)
        {
            var message = $"User {userId} does not exist.";

            _logger.LogInformation(message);
            return Result.Failure<PremiumUser>(message);
        }

        return Result.Success(user);
    }

    public async Task<Result> DeletePreiumUserAsync(Guid userId)
    {
        var result = await _premiumUserCollection
            .DeleteOneAsync(x => x.UserId == userId);

        if(result.DeletedCount == 0) {
            return Result.Failure($"Premium user {userId} was not found.");
        }

        return Result.Success();
    }

    public async Task<bool> IsUserPremiumAsync(Guid userId)
    {
        var user = await _premiumUserCollection
            .Find(x => x.UserId == userId)
            .SingleOrDefaultAsync();

        return user != null && user.PremiumExpiresAt > DateTime.UtcNow;
    }

    public async Task<Result> UpsertPremiumUserExpiryAsync(Guid userId, DateTime premiumExpiresAt)
    {
        var filter = Builders<PremiumUser>.Filter.Eq(x => x.UserId, userId);

        var update = Builders<PremiumUser>.Update
        .Set(x => x.PremiumExpiresAt, premiumExpiresAt)
        .Set(x => x.UpdatedAt, DateTime.UtcNow)
        .SetOnInsert(x => x.CreatedAt, DateTime.UtcNow);

        try
        {
            await _premiumUserCollection.UpdateOneAsync(
            filter,
            update,
            new UpdateOptions { IsUpsert = true });

            return Result.Success();
        }
        catch (Exception ex)
        {
            var message = $"Failed to upsert premium expiry for user {userId}.";
            _logger.LogError(ex, message);
            return Result.Failure(message);
        }
    }
}
