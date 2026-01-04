using CSharpFunctionalExtensions;

namespace Stanmore.Consumer.SubscriptionHandler;

public interface ISubscriptionEventHandler
{
    Task<Result> HandleAsync(SubscriptionEvent subscriptionEvent);
}
