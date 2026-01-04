using System.Text.Json.Serialization;

public class RevenueCatWebhookDto
{
    [JsonPropertyName("api_version")]
    public string ApiVersion { get; set; }

    [JsonPropertyName("event")]
    public RevenueCatEventDto Event { get; set; }
}

public class RevenueCatEventDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("app_user_id")]
    public string AppUserId { get; set; }

    [JsonPropertyName("entitlement_id")]
    public string EntitlementId { get; set; }

    [JsonPropertyName("product_id")]
    public string ProductId { get; set; }

    [JsonPropertyName("expires_at_ms")]
    public long? ExpiresAtMs { get; set; }

    [JsonPropertyName("expiration_at_ms")]
    public long? ExpirationAtMs { get; set; }

    [JsonPropertyName("purchased_at_ms")]
    public long? PurchasedAtMs { get; set; }

    [JsonPropertyName("event_timestamp_ms")]
    public long EventTimestampMs { get; set; }

    [JsonPropertyName("environment")]
    public string Environment { get; set; }

    [JsonPropertyName("store")]
    public string Store { get; set; }

    [JsonPropertyName("subscriber_attributes")]
    public Dictionary<string, SubscriberAttributeDto> SubscriberAttributes { get; set; }
}

public class SubscriberAttributeDto
{
    [JsonPropertyName("updated_at_ms")]
    public long UpdatedAtMs { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}