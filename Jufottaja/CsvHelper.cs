namespace Jufottaja;

public static class CsvHelper
{
    public static Dictionary<HeaderType, List<int>> GetHeaderTypesAndFieldIndexes(string[] fileHeaders, Dictionary<HeaderType, string[]> headers)
    {
        var headerIndexes = new Dictionary<HeaderType, List<int>>();
        
        foreach (var headerType in headers)
        {
            var missingHeaders = headerType.Value.Where(header => !fileHeaders.Contains(header)).ToList();
    
            foreach (var missingHeader in missingHeaders)
            {
                Console.WriteLine($"Could not find specified header {missingHeader} on the first line of the file.");
            }
            
            headerIndexes[headerType.Key] = GetFieldIndexes(fileHeaders, headerType.Value);
        }
        
        return headerIndexes;
    }
    
    // Finds the strings in the 'fields' array that match any of the strings in the 'values' collection
    // and returns the indexes of the matching fields.
    public static List<int> GetFieldIndexes(string[] fields, IEnumerable<string> values)
    {
        return values
            .Select(value => Array.IndexOf(fields, value))
            .Where(index => index != -1)
            .ToList();
    }
    
    public static string? GetFirstOrDefaultInFieldsFromIndexes(IEnumerable<int> indexes, string[] fields)
    {
        return indexes
            .Where(index => !string.IsNullOrEmpty(fields[index]))
            .Select(index => fields[index])
            .FirstOrDefault();
    }
    
    public static string EscapeCsvField(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }
}