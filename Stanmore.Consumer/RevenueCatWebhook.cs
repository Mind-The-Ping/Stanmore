using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stanmore.Consumer.SubscriptionHandler;
using System.Security.Cryptography;
using System.Text.Json;

namespace Stanmore.Consumer;

public class RevenueCatWebhook
{
    private readonly ISubscriptionEventHandler _subscriptionEventHandler;
    private readonly RevenueCatOptions _options;
    private readonly ILogger<RevenueCatWebhook> _logger;

    public RevenueCatWebhook(
        ISubscriptionEventHandler subscriptionEventHandler,
        IOptions<RevenueCatOptions> options,
        ILogger<RevenueCatWebhook> logger)
    {
        _subscriptionEventHandler = subscriptionEventHandler ?? 
            throw new ArgumentNullException(nameof(subscriptionEventHandler));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("RevenueCatWebhook")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("Start processing a RevenueCatWebhook.");


        if (!req.Headers.TryGetValue("X-RevenueCat-Signature", out var signatureValues)) {
            return new UnauthorizedResult();
        }

        var signature = signatureValues.ToString();

        if (!Guid.TryParse(signature, out var signatureId)) {
            return new UnauthorizedResult();
        }

        if (!CryptographicOperations.FixedTimeEquals(
         signatureId.ToByteArray(),
         _options.Signature.ToByteArray())) {
            return new UnauthorizedResult();
        }

        string body;
        using (var reader = new StreamReader(req.Body)) {
            body = await reader.ReadToEndAsync();
        }

        RevenueCatWebhookDto? dto;

        try
        {
            dto = JsonSerializer.Deserialize<RevenueCatWebhookDto>(
                body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize RevenueCat webhook payload.");
            return new OkResult();
        }

        if (dto == null)
        {
            _logger.LogError("RevenueCatWebhookDto deserialized to null.");
            return new OkResult();
        }

         _logger.LogInformation(
            "Processing RevenueCat event {Type} for user {UserId}",
            dto.Event.Type,
            dto.Event.AppUserId);

        var parseResult = SubscriptionEventParser.Parse(dto);

        if (parseResult.IsFailure)
        {
            _logger.LogError(parseResult.Error);
            return new OkResult();
        }

        var handleResult = await _subscriptionEventHandler.HandleAsync(parseResult.Value);
        
        if (handleResult.IsFailure) {
            _logger.LogError(handleResult.Error);
        }

        return new OkResult();
    }
}