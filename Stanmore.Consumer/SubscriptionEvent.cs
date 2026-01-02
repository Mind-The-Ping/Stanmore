namespace Stanmore.Consumer;

public record SubscriptionEvent(
    Guid UserId,
    EventType EventType,
    DateTime? ExpirationDate);