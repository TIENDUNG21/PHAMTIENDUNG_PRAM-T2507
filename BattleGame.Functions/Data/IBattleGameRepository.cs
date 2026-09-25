using BattleGame.Functions.Models;

namespace BattleGame.Functions.Data;

public interface IBattleGameRepository
{
    Task<Player> RegisterPlayerAsync(RegisterPlayerRequest request);
    Task<Asset> CreateAssetAsync(CreateAssetRequest request);
    Task AddPlayerAssetAsync(Guid playerId, Guid assetId);
    Task<IReadOnlyList<PlayerAssetReport>> GetAssetsByPlayerAsync(string? playerName);
}
