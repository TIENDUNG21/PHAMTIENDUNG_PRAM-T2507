using System.Data;
using BattleGame.Functions.Models;
using Microsoft.Data.SqlClient;

namespace BattleGame.Functions.Data;

/// <summary>Data access for the BATTLEGAME database using stored procedures.</summary>
public class BattleGameRepository : IBattleGameRepository
{
    private readonly string _connectionString;

    public BattleGameRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<Player> RegisterPlayerAsync(RegisterPlayerRequest request)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = CreateProcedure(connection, "dbo.usp_RegisterPlayer");
        command.Parameters.Add("@PlayerName", SqlDbType.NVarChar, 64).Value = request.PlayerName!.Trim();
        command.Parameters.Add("@FullName", SqlDbType.NVarChar, 128).Value = ToDbValue(request.FullName);
        command.Parameters.Add("@Age", SqlDbType.NVarChar, 10).Value = ToDbValue(request.Age);
        command.Parameters.Add("@Level", SqlDbType.Int).Value = request.Level ?? 1;
        command.Parameters.Add("@Email", SqlDbType.NVarChar, 64).Value = ToDbValue(request.Email);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new Player
        {
            PlayerId = reader.GetGuid(reader.GetOrdinal("PlayerId")),
            PlayerName = reader.GetString(reader.GetOrdinal("PlayerName")),
            FullName = GetNullableString(reader, "FullName"),
            Age = GetNullableString(reader, "Age"),
            Level = reader.GetInt32(reader.GetOrdinal("Level")),
            Email = GetNullableString(reader, "Email")
        };
    }

    public async Task<Asset> CreateAssetAsync(CreateAssetRequest request)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = CreateProcedure(connection, "dbo.usp_CreateAsset");
        command.Parameters.Add("@AssetName", SqlDbType.NVarChar, 64).Value = request.AssetName!.Trim();
        command.Parameters.Add("@LevelRequire", SqlDbType.Int).Value = request.LevelRequire ?? 1;

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();

        return new Asset
        {
            AssetId = reader.GetGuid(reader.GetOrdinal("AssetId")),
            AssetName = reader.GetString(reader.GetOrdinal("AssetName")),
            LevelRequire = reader.GetInt32(reader.GetOrdinal("LevelRequire"))
        };
    }

    public async Task AddPlayerAssetAsync(Guid playerId, Guid assetId)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = CreateProcedure(connection, "dbo.usp_AddPlayerAsset");
        command.Parameters.Add("@PlayerId", SqlDbType.UniqueIdentifier).Value = playerId;
        command.Parameters.Add("@AssetId", SqlDbType.UniqueIdentifier).Value = assetId;

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<PlayerAssetReport>> GetAssetsByPlayerAsync(string? playerName)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var command = CreateProcedure(connection, "dbo.usp_GetAssetsByPlayer");
        command.Parameters.Add("@PlayerName", SqlDbType.NVarChar, 64).Value = ToDbValue(playerName);

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        var rows = new List<PlayerAssetReport>();
        while (await reader.ReadAsync())
        {
            rows.Add(new PlayerAssetReport
            {
                No = reader.GetInt64(reader.GetOrdinal("No")),
                PlayerName = reader.GetString(reader.GetOrdinal("PlayerName")),
                Level = reader.GetInt32(reader.GetOrdinal("Level")),
                Age = GetNullableString(reader, "Age"),
                AssetName = reader.GetString(reader.GetOrdinal("AssetName"))
            });
        }

        return rows;
    }

    private static SqlCommand CreateProcedure(SqlConnection connection, string procedureName) =>
        new(procedureName, connection) { CommandType = CommandType.StoredProcedure };

    private static object ToDbValue(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim();

    private static string? GetNullableString(SqlDataReader reader, string column)
    {
        var ordinal = reader.GetOrdinal(column);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }
}
