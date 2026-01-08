using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stanmore.Repository.UserRepository;
using System.Security.Claims;

namespace Stanmore.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscriptionController : ControllerBase
{
    private readonly IPremiumUserRepository _repository;
    private readonly ILogger<SubscriptionController> _logger;

    public SubscriptionController(IPremiumUserRepository repository, ILogger<SubscriptionController> logger)
    {
        _repository = repository ?? 
            throw new ArgumentNullException(nameof(repository));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Authorize]
    [HttpGet("premiumUser")]
    public async Task<IActionResult> GetPremiumUser()
    {
        var subValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(subValue, out var userId))
        {
            _logger.LogError("User could not log in with {userId}", subValue);
            return BadRequest("Can not access endpoint without logging in.");
        }

        var result = await _repository.IsUserPremiumAsync(userId);

        return Ok(new {isUserPremium = result});
    }

    [Authorize]
    [HttpGet("premiumUserById")]
    public async Task<IActionResult> GetPremiumUserById(Guid userId) =>
        Ok(await _repository.IsUserPremiumAsync(userId));
}
