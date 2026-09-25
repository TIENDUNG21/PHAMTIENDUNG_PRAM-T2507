namespace BattleGame.Functions.Models;

public class RegisterPlayerRequest
{
    public string? PlayerName { get; set; }
    public string? FullName { get; set; }
    public string? Age { get; set; }
    public int? Level { get; set; }
    public string? Email { get; set; }
}

public class CreateAssetRequest
{
    public string? AssetName { get; set; }
    public int? LevelRequire { get; set; }
}

public class AddPlayerAssetRequest
{
    public Guid? PlayerId { get; set; }
    public Guid? AssetId { get; set; }
}
