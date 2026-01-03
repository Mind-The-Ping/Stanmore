using CSharpFunctionalExtensions;

namespace Stanmore.Consumer;

public class SubscriptionEventParser
{
    public static Result<SubscriptionEvent> Parse(SubscriptionEventDto subscriptionEventDto)
    {
        if (subscriptionEventDto == null) {
            return Result.Failure<SubscriptionEvent>("The event was null.");
        }

        if (subscriptionEventDto._event == null) {
            return Result.Failure<SubscriptionEvent>("Event payload was null.");
        }

        if (!Guid.TryParse(subscriptionEventDto._event.app_user_id, out var userId)) 
        {
            return Result.Failure<SubscriptionEvent>(
                $"Could not parse {subscriptionEventDto._event.app_user_id} as a guid.");
        }

        DateTime? expirationDate = subscriptionEventDto._event.expiration_at_ms == 0 ? 
             null : 
             DateTimeOffset
            .FromUnixTimeMilliseconds(subscriptionEventDto._event.expiration_at_ms)
            .UtcDateTime;

        var eventType = MapEventType(subscriptionEventDto._event.type);

        if (eventType.IsFailure) {
            return Result.Failure<SubscriptionEvent>(eventType.Error);
        }

        return new SubscriptionEvent(userId, eventType.Value, expirationDate);
    }

    public static Result<EventType> MapEventType(string type)
    {
        return type switch
        {
            "TEST" => EventType.Test,
            "INITIAL_PURCHASE" => EventType.InitialPurchase,
            "RENEWAL" => EventType.Renewal,
            "CANCELLATION" => EventType.Cancellation,
            "UNCANCELLATION" => EventType.UnCancellation,
            "NON_RENEWING_PURCHASE" => EventType.NonRenewingPurchase,
            "SUBSCRIPTION_PAUSED" => EventType.SubscriptionPaused,
            "EXPIRATION" => EventType.Expiration,
            "BILLING_ISSUE" => EventType.BillingIssue,
            "PRODUCT_CHANGE" => EventType.ProductChange,
            "TRANSFER" => EventType.Transfer,
            "SUBSCRIPTION_EXTENDED" => EventType.SubscriptionExtended,
            "TEMPORARY_ENTITLEMENT_GRANT" => EventType.TemporaryEntitlementGrant,
            "REFUND_REVERSED" => EventType.RefundReversed,
            "VIRTUAL_CURRENCY_TRANSACTION" => EventType.VirtualCurrencyTransaction,
            "EXPERIMENT_ENROLLMENT" => EventType.Experiment_Enrollment,
            _ => Result.Failure<EventType>($"Unhandled RevenueCat event type: {type}.")
        };
    }
}
