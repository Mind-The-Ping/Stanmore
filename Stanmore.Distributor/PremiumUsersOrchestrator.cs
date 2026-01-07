using Microsoft.Extensions.Logging;
using Stanmore.Repository.UserRepository;

namespace Stanmore.Distributor;

public class PremiumUsersOrchestrator
{
    private readonly IPremiumUserRepository _premiumUserRepository;
    private readonly ILogger<PremiumUsersOrchestrator> _logger;

    public PremiumUsersOrchestrator(
        IPremiumUserRepository premiumUserRepository, 
        ILogger<PremiumUsersOrchestrator> logger)
    {
        _premiumUserRepository = premiumUserRepository ??
            throw new ArgumentNullException(nameof(premiumUserRepository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
