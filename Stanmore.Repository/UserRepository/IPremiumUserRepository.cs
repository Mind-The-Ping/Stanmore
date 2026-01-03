using CSharpFunctionalExtensions;

namespace Stanmore.Repository.UserRepository;

public interface IPremiumUserRepository
{
    public Task<bool> IsUserPremiumAsync(Guid userId);
    public Task<Result> AddPremiumUserAsync(PremiumUser premiumUser);
    public Task<Result> UpdatePremiumUserExpiryTimeAsync(Guid userId, DateTime premiumExpires);
    public Task<Result> DeletePreiumUserAsync(Guid userId);
    public Task<Result<PremiumUser>> GetPremiumUserAsync(Guid userId);
}
