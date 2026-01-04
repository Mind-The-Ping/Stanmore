using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stanmore.Consumer.SubscriptionHandler;
using System.Net;
using System.Security.Cryptography;
using System.Text;
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
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
    {
        _logger.LogInformation("Start processing a RevenueCatWebhook.");


        if (!req.Headers.TryGetValues("Authorization", out var authValues))
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }


        var authHeader = authValues.First();

        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
        }

        var receivedSecret = authHeader["Bearer ".Length..].Trim();
        var expectedSecret = _options.Signature.Trim();

        if (!CryptographicOperations.FixedTimeEquals(
          Encoding.UTF8.GetBytes(receivedSecret),
          Encoding.UTF8.GetBytes(expectedSecret)))
        {
            return req.CreateResponse(HttpStatusCode.Unauthorized);
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
            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        if (dto == null)
        {
            _logger.LogError("RevenueCatWebhookDto deserialized to null.");
            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

         _logger.LogInformation(
            "Processing RevenueCat event {Type} for user {UserId}",
            dto.Event?.Type,
            dto.Event?.AppUserId);

        var parseResult = SubscriptionEventParser.Parse(dto);

        if (parseResult.IsFailure)
        {
            _logger.LogError(parseResult.Error);
            return req.CreateResponse(System.Net.HttpStatusCode.OK);
        }

        var handleResult = await _subscriptionEventHandler.HandleAsync(parseResult.Value);
        
        if (handleResult.IsFailure) {
            _logger.LogError(handleResult.Error);
        }

        return req.CreateResponse(System.Net.HttpStatusCode.OK);
    }
}