using BattleGame.Functions.Data;
using BattleGame.Functions.Helpers;
using BattleGame.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BattleGame.Functions.Functions;

public class CreateAssetFunction
{
    private readonly IBattleGameRepository _repository;
    private readonly ILogger<CreateAssetFunction> _logger;

    public CreateAssetFunction(IBattleGameRepository repository, ILogger<CreateAssetFunction> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>POST /api/createasset - admin creates a new asset.</summary>
    [Function("createasset")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "createasset")] HttpRequest req)
    {
        var request = await RequestHelper.ReadJsonBodyAsync<CreateAssetRequest>(req);
        if (request is null || string.IsNullOrWhiteSpace(request.AssetName))
        {
            return new BadRequestObjectResult(new { message = "AssetName is required." });
        }

        if (request.LevelRequire is < 1)
        {
            return new BadRequestObjectResult(new { message = "LevelRequire must be greater than 0." });
        }

        var asset = await _repository.CreateAssetAsync(request);
        _logger.LogInformation("Created asset {AssetName} ({AssetId})", asset.AssetName, asset.AssetId);
        return new ObjectResult(asset) { StatusCode = StatusCodes.Status201Created };
    }
}
