using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace BattleGame.Functions.Helpers;

public static class RequestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Reads the JSON body of a request. Returns null if the body is empty or invalid.</summary>
    public static async Task<T?> ReadJsonBodyAsync<T>(HttpRequest request) where T : class
    {
        try
        {
            return await JsonSerializer.DeserializeAsync<T>(request.Body, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
