using BattleGame.Functions.Data;
using BattleGame.Functions.Helpers;
using BattleGame.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;

namespace BattleGame.Functions.Functions;

public class AddPlayerAssetFunction
{
    private const int UniqueViolationError = 2627;
    private const int ForeignKeyViolationError = 547;

    private readonly IBattleGameRepository _repository;

    public AddPlayerAssetFunction(IBattleGameRepository repository)
    {
        _repository = repository;
    }

    /// <summary>POST /api/addplayerasset - gives an asset to a player (fills the PlayerAsset table).</summary>
    [Function("addplayerasset")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addplayerasset")] HttpRequest req)
    {
        var request = await RequestHelper.ReadJsonBodyAsync<AddPlayerAssetRequest>(req);
        if (request?.PlayerId is null || request.AssetId is null)
        {
            return new BadRequestObjectResult(new { message = "PlayerId and AssetId are required." });
        }

        try
        {
            await _repository.AddPlayerAssetAsync(request.PlayerId.Value, request.AssetId.Value);
            return new ObjectResult(request) { StatusCode = StatusCodes.Status201Created };
        }
        catch (SqlException ex) when (ex.Number == UniqueViolationError)
        {
            return new ConflictObjectResult(new { message = "The player already owns this asset." });
        }
        catch (SqlException ex) when (ex.Number == ForeignKeyViolationError)
        {
            return new NotFoundObjectResult(new { message = "Player or asset does not exist." });
        }
    }
}
