using CSharpFunctionalExtensions;

namespace Stanmore.Consumer;

public class SubscriptionEventParser
{
    public static Result<SubscriptionEvent> Parse(RevenueCatWebhookDto webhookDto)
    {
        if (webhookDto  == null) {
            return Result.Failure<SubscriptionEvent>("The event was null.");
        }

        if (webhookDto.Event == null) {
            return Result.Failure<SubscriptionEvent>("Event payload was null.");
        }

        if (string.IsNullOrWhiteSpace(webhookDto.Event.AppUserId)) {
            return Result.Failure<SubscriptionEvent>("app_user_id was missing.");
        }

        if (!Guid.TryParse(webhookDto.Event.AppUserId, out var userId)) 
        {
            return Result.Failure<SubscriptionEvent>(
                $"Could not parse {webhookDto.Event.AppUserId} as a guid.");
        }

        var eventType = MapEventType(webhookDto.Event.Type);

        if (eventType.IsFailure) {
            return Result.Failure<SubscriptionEvent>(eventType.Error);
        }

        DateTime? expirationDate = null;

        var expirationMs =
            webhookDto.Event.ExpiresAtMs ??
            webhookDto.Event.ExpirationAtMs;

        if (expirationMs.HasValue)
        {
            expirationDate = DateTimeOffset
                .FromUnixTimeMilliseconds(expirationMs.Value)
                .UtcDateTime;
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
            "VIRTUAL_CURRENCY_TRANSACTION" => EventType.VirtualCurrencyTransaction,
            "EXPERIMENT_ENROLLMENT" => EventType.Experiment_Enrollment,
            _ => Result.Failure<EventType>($"Unhandled RevenueCat event type: {type}.")
        };
    }
}
