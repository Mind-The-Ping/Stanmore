using CSharpFunctionalExtensions;
using Stanmore.Repository.UserRepository;

namespace Stanmore.Consumer.SubscriptionHandler;

public class SubscriptionEventHandler : ISubscriptionEventHandler
{
    private readonly IPremiumUserRepository _premiumUserRepository;

    public SubscriptionEventHandler(IPremiumUserRepository premiumUserRepository)
    {
        _premiumUserRepository = premiumUserRepository ??
            throw new ArgumentNullException(nameof(premiumUserRepository));
    }

    public async Task<Result> HandleAsync(SubscriptionEvent subscriptionEvent)
    {
        switch (subscriptionEvent.EventType)
        {
            case EventType.InitialPurchase:
            case EventType.Renewal:
            case EventType.UnCancellation:
            case EventType.SubscriptionExtended:
            case EventType.TemporaryEntitlementGrant:
                return await HandlePremiumGrantAsync(subscriptionEvent);

            case EventType.Expiration:
                return await HandleExpirationAsync(subscriptionEvent);

            case EventType.Test:
            case EventType.Cancellation:
            case EventType.NonRenewingPurchase:
            case EventType.SubscriptionPaused:
            case EventType.BillingIssue:
            case EventType.ProductChange:
            case EventType.Transfer:
            case EventType.VirtualCurrencyTransaction:
            case EventType.Experiment_Enrollment:
                return Result.Success();

            default:
                return Result.Success();
        }
    }

    private async Task<Result> HandlePremiumGrantAsync(SubscriptionEvent subscriptionEvent)
    {
        if (subscriptionEvent.ExpirationDate == null)
        {
            return Result.Failure(
                $"Event {subscriptionEvent.EventType} requires an expiration date but none was provided.");
        }

        return await _premiumUserRepository.UpsertPremiumUserExpiryAsync(
           subscriptionEvent.UserId,
           subscriptionEvent.ExpirationDate.Value
       );
    }

    private async Task<Result> HandleExpirationAsync(SubscriptionEvent evt)
    {
        return await _premiumUserRepository.UpsertPremiumUserExpiryAsync(
            evt.UserId,
            DateTime.UtcNow
        );
    }
}
