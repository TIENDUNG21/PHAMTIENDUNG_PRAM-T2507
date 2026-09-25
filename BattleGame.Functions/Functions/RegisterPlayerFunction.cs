using BattleGame.Functions.Data;
using BattleGame.Functions.Helpers;
using BattleGame.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public class RegisterPlayerFunction
{
    private const int UniqueViolationError = 2627;

    private readonly IBattleGameRepository _repository;
    private readonly ILogger<RegisterPlayerFunction> _logger;

    public RegisterPlayerFunction(IBattleGameRepository repository, ILogger<RegisterPlayerFunction> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>POST /api/registerplayer - registers a new player.</summary>
    [Function("registerplayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "registerplayer")] HttpRequest req)
    {
        var request = await RequestHelper.ReadJsonBodyAsync<RegisterPlayerRequest>(req);
        if (request is null || string.IsNullOrWhiteSpace(request.PlayerName))
        {
            return new BadRequestObjectResult(new { message = "PlayerName is required." });
        }

        if (request.Level is < 1)
        {
            return new BadRequestObjectResult(new { message = "Level must be greater than 0." });
        }

        try
        {
            var player = await _repository.RegisterPlayerAsync(request);
            _logger.LogInformation("Registered player {PlayerName} ({PlayerId})", player.PlayerName, player.PlayerId);
            return new ObjectResult(player) { StatusCode = StatusCodes.Status201Created };
        }
        catch (SqlException ex) when (ex.Number == UniqueViolationError)
        {
            return new ConflictObjectResult(new { message = $"Player name '{request.PlayerName}' already exists." });
        }
    }
}
