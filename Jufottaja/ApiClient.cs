using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Jufottaja;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://jufo-rest.csc.fi/v1.1";

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    
    /// <returns>
    /// Jufo_ID as string if found,
    /// "NO RESULT" if Jufo API returned nothing,
    /// "MULTIPLE RESULTS" if Jufo API returned more than one result
    /// "FAILURE" if parameter validation did not go through or if the Jufo API return cannot be parsed.
    /// </returns>
    public async Task<string> GetJufoChannelId(JufoApiQueryParameters parameters)
    {
        if (!AreQueryParametersValid(parameters))
        {
            return "FAILURE";
        }
        
        var queryString = BuildQueryString(parameters);
        try
        {
            var url = $"{BaseUrl}/etsi.php?{queryString}";
            var response = await _httpClient.GetStringAsync(url);
            
            if (string.IsNullOrEmpty(response))
            {
                return "NO RESULT";
            }
            
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() <= 0)
            {
                return "NO RESULT";
            }

            if (root.GetArrayLength() > 1)
            {
                Console.WriteLine($"There are multiple results from Jufo for {parameters}");
                Console.WriteLine("Check Jufo manually for the correct one. Skipping...");
                return "MULTIPLE RESULTS";
            }

            var firstItem = root[0];
            if (firstItem.TryGetProperty("Jufo_ID", out var jufoIdProperty))
            {
                return jufoIdProperty.GetString() ?? "FAILURE";
            }
            
            return "FAILURE";
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
            throw new Exception("Error while calling the JUFO API.", ex);
        }
    }

    private static bool AreQueryParametersValid(JufoApiQueryParameters parameters)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(parameters);
        
        if (!Validator.TryValidateObject(parameters, validationContext, validationResults, true))
        {
            foreach (var validationResult in validationResults)
            {
                Console.WriteLine($"Validation Error: {validationResult.ErrorMessage}");
            }
        }

        return validationResults.Count == 0;
    }
    
    private static string BuildQueryString(JufoApiQueryParameters parameters)
    {
        var queryParams = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(parameters.Name))
            queryParams["nimi"] = parameters.Name;
        if (!string.IsNullOrEmpty(parameters.Isbn))
            queryParams["isbn"] = parameters.Isbn;
        if (!string.IsNullOrEmpty(parameters.Issn))
            queryParams["issn"] = parameters.Issn;
        if (!string.IsNullOrEmpty(parameters.ConferenceAbbreviation))
            queryParams["lyhenne"] = parameters.ConferenceAbbreviation;

        return string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
    }
}