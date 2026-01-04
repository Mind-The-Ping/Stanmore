using FluentAssertions;
using NSubstitute;
using Stanmore.Consumer.SubscriptionHandler;
using Stanmore.Repository.UserRepository;

namespace Stanmore.Consumer.UnitTests;

public class SubscriptionEventHandlerTests
{
    private readonly IPremiumUserRepository _userRepository;
    private readonly SubscriptionEventHandler _subscriptionEventHandler;

    public SubscriptionEventHandlerTests()
    {
        _userRepository = NSubstitute.Substitute.For<IPremiumUserRepository>();
        _subscriptionEventHandler = new SubscriptionEventHandler(_userRepository);
    }

    [Fact]
    public void SubscriptionEventHandler_Ctor_NullRepository_Throws_ArguementNullException()
    {
        var exception = Assert
            .Throws<ArgumentNullException>(() => new SubscriptionEventHandler(
                null!));

        exception.Message.Should().Be("Value cannot be null. (Parameter 'premiumUserRepository')");
    }

    [Theory]
    [InlineData(EventType.Test)]
    [InlineData(EventType.Cancellation)]
    [InlineData(EventType.NonRenewingPurchase)]
    [InlineData(EventType.SubscriptionPaused)]
    [InlineData(EventType.BillingIssue)]
    [InlineData(EventType.ProductChange)]
    [InlineData(EventType.Transfer)]
    [InlineData(EventType.VirtualCurrencyTransaction)]
    [InlineData(EventType.Experiment_Enrollment)]
    public async Task SubscriptionEventHandler_HandleAsync_Ingored_Type_Successful(EventType eventType)
    {
        var subscriptionEvent = new SubscriptionEvent(Guid.NewGuid(), eventType, DateTime.UtcNow);

        var result = await _subscriptionEventHandler.HandleAsync(subscriptionEvent);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SubscriptionEventHandler_HandleAsync_Expiration_Type_Successful()
    {
        var subscriptionEvent = new SubscriptionEvent(Guid.NewGuid(), EventType.Expiration, DateTime.UtcNow.AddDays(30));

        var before = DateTime.UtcNow;

        var result = await _subscriptionEventHandler.HandleAsync(subscriptionEvent);

        var after = DateTime.UtcNow;

        result.IsSuccess.Should().BeTrue();

        await _userRepository.Received(1)
        .UpsertPremiumUserExpiryAsync(
            subscriptionEvent.UserId,
            Arg.Is<DateTime>(dt => dt >= before && dt <= after));
    }

    [Theory]
    [InlineData(EventType.InitialPurchase)]
    [InlineData(EventType.Renewal)]
    [InlineData(EventType.UnCancellation)]
    [InlineData(EventType.SubscriptionExtended)]
    [InlineData(EventType.TemporaryEntitlementGrant)]
    public async Task SubscriptionEventHandler_HandleAsync_Extend_ExpirationType_Type_Successful(EventType eventType)
    {
        var subscriptionEvent = new SubscriptionEvent(Guid.NewGuid(), eventType, DateTime.UtcNow.AddDays(30));

        DateTime? capturedExpiry = null;

        _userRepository
            .When(x => x.UpsertPremiumUserExpiryAsync(
                subscriptionEvent.UserId,
                Arg.Any<DateTime>()))
            .Do(callInfo => capturedExpiry = callInfo.ArgAt<DateTime>(1));

        var result = await _subscriptionEventHandler.HandleAsync(subscriptionEvent);

        result.IsSuccess.Should().BeTrue();

        await _userRepository.Received(1)
       .UpsertPremiumUserExpiryAsync(
           subscriptionEvent.UserId,
           Arg.Any<DateTime>());

        capturedExpiry.Should()
            .BeCloseTo(subscriptionEvent.ExpirationDate!.Value, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task SubscriptionEventHandler_HandleAsync_Extend_Null_Expiration_Fails()
    {
        var subscriptionEvent = new SubscriptionEvent(Guid.NewGuid(), EventType.InitialPurchase, null);

        var result = await _subscriptionEventHandler.HandleAsync(subscriptionEvent);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be($"Event {subscriptionEvent.EventType} requires an expiration date but none was provided.");
    }
}
