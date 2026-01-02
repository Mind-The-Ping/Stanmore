using FluentAssertions;

namespace Stanmore.Consumer.UnitTests;

public class SubscriptionEventParserTest
{
    [Fact]
    public void SubscriptionEventParser_Parse_Successful()
    {
        var userId = Guid.NewGuid();

        var subscriptionDto = new SubscriptionEventDto
        {
            _event = new Event()
        };

        subscriptionDto._event.app_user_id = userId.ToString();
        subscriptionDto._event.type = "TEST";
        subscriptionDto._event.expiration_at_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(subscriptionDto);
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.EventType.Should().Be(EventType.Test);
        result.Value.ExpirationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void SubscriptionEventParser_Parse_Fails_Null_Event()
    {
        var result = SubscriptionEventParser.Parse(null!);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("The event was null.");
    }

    [Fact]
    public void SubscriptionEventParser_Parse_Fails_Payload_Null_Event()
    {
        var subscriptionDto = new SubscriptionEventDto();

        var result = SubscriptionEventParser.Parse(subscriptionDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Event payload was null.");
    }

    [Fact]
    public void SubscriptionEventParser_Parse_Fails_InCorrect_UserId()
    {
        var wrongId = "12345";

        var subscriptionDto = new SubscriptionEventDto
        {
            _event = new Event()
        };

        subscriptionDto._event.app_user_id = wrongId;
        subscriptionDto._event.type = "TEST";
        subscriptionDto._event.expiration_at_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(subscriptionDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Could not parse {wrongId} as a guid.");
    }

    [Fact]
    public void SubscriptionEventParser_Parse_Expiration_0_Null_ExpirationDate()
    {
        var userId = Guid.NewGuid();

        var subscriptionDto = new SubscriptionEventDto
        {
            _event = new Event()
        };

        subscriptionDto._event.app_user_id = userId.ToString();
        subscriptionDto._event.type = "TEST";
        subscriptionDto._event.expiration_at_ms = 0;

        var result = SubscriptionEventParser.Parse(subscriptionDto);
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.EventType.Should().Be(EventType.Test);
        result.Value.ExpirationDate.Should().BeNull();
    }

    [Fact]
    public void SubscriptionEventParser_Parse_EventType_Not_There_Fails()
    {
        var userId = Guid.NewGuid();
        var eventType = "FREE";

        var subscriptionDto = new SubscriptionEventDto
        {
            _event = new Event()
        };

        subscriptionDto._event.app_user_id = userId.ToString();
        subscriptionDto._event.type = eventType;
        subscriptionDto._event.expiration_at_ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(subscriptionDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Unhandled RevenueCat event type: {eventType}.");
    }
}
