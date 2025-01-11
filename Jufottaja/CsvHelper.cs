namespace Jufottaja;

public static class CsvHelper
{
    // Finds the strings in the 'fields' array that match any of the strings in the 'values' collection
    // and returns the indexes of the matching fields.
    public static List<int> GetFieldIndexes(string[] fields, IEnumerable<string> values)
    {
        var indexes = values
            .Select(value => Array.IndexOf(fields, value))
            .Where(index => index != -1)
            .ToList();

        return indexes;
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