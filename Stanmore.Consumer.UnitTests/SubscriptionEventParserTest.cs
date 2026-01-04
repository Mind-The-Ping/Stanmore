using FluentAssertions;

namespace Stanmore.Consumer.UnitTests;

public class SubscriptionEventParserTest
{
    [Fact]
    public void SubscriptionEventParser_Parse_Successful()
    {
        var userId = Guid.NewGuid();

        var webhookDto = new RevenueCatWebhookDto
        {
            Event = new RevenueCatEventDto()
        };

        webhookDto.Event.AppUserId = userId.ToString();
        webhookDto.Event.Type = "TEST";
        webhookDto.Event.ExpiresAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(webhookDto);
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
        var webhookDto = new RevenueCatWebhookDto();

        var result = SubscriptionEventParser.Parse(webhookDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Event payload was null.");
    }

    [Fact]
    public void SubscriptionEventParser_Parse_Fails_InCorrect_UserId()
    {
        var wrongId = "12345";

        var webhookDto = new RevenueCatWebhookDto
        {
            Event = new RevenueCatEventDto()
        };

        webhookDto.Event.AppUserId = wrongId;
        webhookDto.Event.Type = "TEST";
        webhookDto.Event.ExpiresAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(webhookDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Could not parse {wrongId} as a guid.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null!)]
    public void SubscriptionEventParser_Parse_Fails_Blank_UserId(string? wrongId)
    {
        var webhookDto = new RevenueCatWebhookDto
        {
            Event = new RevenueCatEventDto()
        };

        webhookDto.Event.AppUserId = wrongId!;
        webhookDto.Event.Type = "TEST";
        webhookDto.Event.ExpiresAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(webhookDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("app_user_id was missing.");
    }

    [Fact]
    public void SubscriptionEventParser_Parse_EventType_Not_There_Fails()
    {
        var userId = Guid.NewGuid();
        var eventType = "FREE";

        var webhookDto = new RevenueCatWebhookDto
        {
            Event = new RevenueCatEventDto()
        };

        webhookDto.Event.AppUserId = userId.ToString();
        webhookDto.Event.Type = eventType;
        webhookDto.Event.ExpiresAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var result = SubscriptionEventParser.Parse(webhookDto);
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Unhandled RevenueCat event type: {eventType}.");
    }
}
