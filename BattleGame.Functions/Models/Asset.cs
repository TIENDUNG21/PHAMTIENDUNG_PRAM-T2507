namespace BattleGame.Functions.Models;

public class Asset
{
    public Guid AssetId { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public int LevelRequire { get; set; }
}
