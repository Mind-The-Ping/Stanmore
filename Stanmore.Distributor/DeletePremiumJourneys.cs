using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stanmore.Repository.UserRepository;

namespace Stanmore.Distributor;

public class DeletePremiumJourneys
{
    private readonly ILogger _logger;
    private readonly IPremiumUserRepository _repository;
    private readonly ServiceBusSender _deleteJourneySender;

    public DeletePremiumJourneys(
        IPremiumUserRepository repository,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ServiceBusClient serviceBusClient,
        ILoggerFactory loggerFactory)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

        var options = serviceBusOptions.Value ?? throw new ArgumentNullException(nameof(serviceBusOptions));

        ServiceBusSender CreateSender(string name, string propertyPath)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException($"ServiceBus entity '{propertyPath}' is not configured.");
            return serviceBusClient.CreateSender(name);
        }

        _deleteJourneySender = CreateSender(options.DeleteJourneys, "ServiceBus:Queues:DeleteJourneys");

        _logger = loggerFactory.CreateLogger<DeletePremiumJourneys>();
    }

    [Function("DeletePremiumJourneys")]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation("DeletePremiumJourneys executed at: {executionTimeUtc}", now);

        var cutoff = now.AddHours(-24);

        var expiredUsers = await _repository.GetExpiredUsersAsync(cutoff);

        if(!expiredUsers.Any()) 
        {
            _logger.LogInformation("There are no expired users to clean up at: {executionTime}", DateTime.Now);
            return;
        }

        foreach (var user in expiredUsers)
        {
            try
            {
                var payload = new { userId = user.UserId };
                var body = BinaryData.FromObjectAsJson(payload);

                var sbMessage = new ServiceBusMessage(body)
                {
                    MessageId = $"{user.UserId}:premium_cleanup"
                };

                await _deleteJourneySender.SendMessageAsync(sbMessage);

                var result = await _repository.MarkUserCleanUpCompletedAsync(user.UserId);

                if(result.IsFailure) {
                    _logger.LogError("Could not mark user {userId} as cleanup completed: {error}", user.UserId,result.Error);
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Could not send delete journey message for {userId}.", user.UserId);
            }
        }
    }
}