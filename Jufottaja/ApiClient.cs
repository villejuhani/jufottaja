using System.Text.Json;
using System.Text.RegularExpressions;

namespace Jufottaja;

public partial class ApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://jufo-rest.csc.fi/v1.1";

    public ApiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> GetJufoChannelId(string name)
    {
        try
        {
            if (!IsValidName(name))
            {
                Console.WriteLine($"Invalid name parameter: '{name}'. Name must contain only a-Z, 0-9 and '-'");
                return "";
            }
            
            var url = $"{BaseUrl}/etsi.php?nimi={Uri.EscapeDataString(name)}";
            var response = await _httpClient.GetStringAsync(url);
            
            if (string.IsNullOrEmpty(response) || response == "{}")
            {
                return "";
            }
            
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() <= 0)
            {
                return "";
            }

            var firstItem = root[0];
            if (firstItem.TryGetProperty("Jufo_ID", out var jufoIdProperty))
            {
                return jufoIdProperty.GetString() ?? "";
            }

            return "";
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Error while calling the JUFO API.", ex);
        }
    }
    
    public async Task<string> GetJufoChannelLevel(string channelId)
    {
        try
        {
            var url = $"{BaseUrl}/kanava/{channelId}";
            var response = await _httpClient.GetStringAsync(url);
            
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;
            
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() <= 0)
            {
                return "";
            }

            var firstItem = root[0];
            if (firstItem.TryGetProperty("Level", out var levelProperty))
            {
                return levelProperty.GetString() ?? "";
            }
            return "";
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Network error while calling the JUFO API.", ex);
        }
    }

    private static readonly Regex NameValidationRegex = MyRegex();
    [GeneratedRegex("^[a-zA-Z0-9-]+$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
    private static bool IsValidName(string name)
    {
        return NameValidationRegex.IsMatch(name);
    }
}