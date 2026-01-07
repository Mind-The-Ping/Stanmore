using CSharpFunctionalExtensions;

namespace Stanmore.Repository.UserRepository;

public interface IPremiumUserRepository
{
    public Task<bool> IsUserPremiumAsync(Guid userId);
    public Task<Result> DeletePreiumUserAsync(Guid userId);
    public Task<Result<PremiumUser>> GetPremiumUserAsync(Guid userId);
    public Task<Result> UpsertPremiumUserExpiryAsync(Guid userId, DateTime premiumExpiresAt);
    public Task<IEnumerable<PremiumUser>> GetExpiredUsersAsync(DateTime cutoff);
    public Task<Result> MarkUserCleanUpCompletedAsync(Guid userId);
}
