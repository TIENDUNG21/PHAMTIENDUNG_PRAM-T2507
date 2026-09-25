namespace BattleGame.Functions.Models;

/// <summary>One row of the "assets by player" report.</summary>
public class PlayerAssetReport
{
    public long No { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string? Age { get; set; }
    public string AssetName { get; set; } = string.Empty;
}
