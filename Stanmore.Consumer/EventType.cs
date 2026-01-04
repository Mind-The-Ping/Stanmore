namespace Stanmore.Consumer;

public enum EventType
{
    Test,
    InitialPurchase,
    Renewal,
    Cancellation,
    UnCancellation,
    NonRenewingPurchase,
    SubscriptionPaused,
    Expiration,
    BillingIssue,
    ProductChange,
    Transfer,
    SubscriptionExtended,
    TemporaryEntitlementGrant,
    VirtualCurrencyTransaction,
    Experiment_Enrollment,
}
