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
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(mongoDatabase);

        _premiumUserCollection = mongoDatabase.GetCollection<PremiumUser>(options.Value.Collection);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> AddPremiumUserAsync(PremiumUser premiumUser)
    {
        try {
            await _premiumUserCollection.InsertOneAsync(premiumUser);
        }
        catch (Exception ex)
        {
            var message = $"Could not save the premium user: {premiumUser.UserId}.";

            _logger.LogError(ex, message);
            return Result.Failure(message);
        }

        return Result.Success();
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

    public async Task<Result> UpdatePremiumUserExpiryTimeAsync(Guid userId, DateTime premiumExpires)
    {
        var update = Builders<PremiumUser>.Update
               .Set(x => x.PremiumExpiresAt, premiumExpires);

        try
        {
            var updatedUser = await _premiumUserCollection.FindOneAndUpdateAsync(
             x => x.UserId == userId,
             update,
             new FindOneAndUpdateOptions<PremiumUser>
             {
                 ReturnDocument = ReturnDocument.After
             });

            if (updatedUser == null) {
                return Result.Failure<string>($"{userId} does not exist.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            var message = $"Database could not save the new expiry date for user {userId}.";

            _logger.LogError(ex, message);
            return Result.Failure<string>(message);
        }
    }
}
