using BattleGame.Functions.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace BattleGame.Functions.Functions;

public class GetAssetsByPlayerFunction
{
    private readonly IBattleGameRepository _repository;

    public GetAssetsByPlayerFunction(IBattleGameRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// GET /api/getassetsbyplayer[?playerName=Player 1] - report of assets owned by players.
    /// </summary>
    [Function("getassetsbyplayer")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "getassetsbyplayer")] HttpRequest req)
    {
        string? playerName = req.Query["playerName"];
        var report = await _repository.GetAssetsByPlayerAsync(playerName);
        return new OkObjectResult(report);
    }
}
