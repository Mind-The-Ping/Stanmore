
public class SubscriptionEvent
{
    public string api_version { get; set; }
    public Event _event { get; set; }
}

public class Event
{
    public string[] aliases { get; set; }
    public string app_id { get; set; }
    public string app_user_id { get; set; }
    public float commission_percentage { get; set; }
    public string country_code { get; set; }
    public string currency { get; set; }
    public string entitlement_id { get; set; }
    public string[] entitlement_ids { get; set; }
    public string environment { get; set; }
    public long event_timestamp_ms { get; set; }
    public long expiration_at_ms { get; set; }
    public string id { get; set; }
    public bool is_family_share { get; set; }
    public string offer_code { get; set; }
    public string original_app_user_id { get; set; }
    public string original_transaction_id { get; set; }
    public string period_type { get; set; }
    public string presented_offering_id { get; set; }
    public float price { get; set; }
    public float price_in_purchased_currency { get; set; }
    public string product_id { get; set; }
    public long purchased_at_ms { get; set; }
    public string store { get; set; }
    public Subscriber_Attributes subscriber_attributes { get; set; }
    public float takehome_percentage { get; set; }
    public float tax_percentage { get; set; }
    public string transaction_id { get; set; }
    public string type { get; set; }
}

public class Subscriber_Attributes
{
    public FavoriteCat FavoriteCat { get; set; }
}

public class FavoriteCat
{
    public long updated_at_ms { get; set; }
    public string value { get; set; }
}