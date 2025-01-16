namespace Jufottaja;

public static class ConsoleHelper
{
    public static Dictionary<HeaderType, string[]> GetHeaders()
    {
        Console.WriteLine("Specify the headers in your file used for finding Publication Channel in Jufo. Do not use dash '-' in the headers.");
        Console.WriteLine("You have to specify the type aswell. The possible types are: name, isbn, issn, conferenceAbbreviation.");
        Console.WriteLine("Example use: -name:Publisher,title,Conference Name -isbn:ISBN -issn:ISSN -type:Item Type");
        Console.WriteLine("Enter your headers:");
        
        var input = Console.ReadLine();
        var columnHeaders = new Dictionary<HeaderType, string[]>();
    
        if (string.IsNullOrEmpty(input))
        {
            return columnHeaders;
        }
        
        columnHeaders = ExtractParameters(input);
    
        foreach (var kvp in columnHeaders)
        {
            Console.WriteLine($"For: {kvp.Key}, using: {string.Join(", ", kvp.Value)}");
        }
    
        return columnHeaders;
    }

    private static Dictionary<HeaderType, string[]> ExtractParameters(string input)
    {
        var parameters = new Dictionary<HeaderType, string[]>();

        if (string.IsNullOrWhiteSpace(input))
        {
            return parameters;
        }
    
        var currentIndex = 0;
        while (currentIndex < input.Length)
        {
            var paramStart = input.IndexOf('-', currentIndex);
            if (paramStart == -1)
            {
                break;
            }
    
            var paramEnd = input.IndexOf(':', paramStart);
            if (paramEnd == -1)
            {
                break;
            }
    
            var paramName = input.Substring(paramStart + 1, paramEnd - paramStart - 1);
    
            if (!HeaderTypeHelper.TryParseHeaderType(paramName, out var headerType))
            {
                Console.WriteLine($"Unknown parameter: {paramName}. Skipping.");
                currentIndex = input.IndexOf('-', paramEnd + 1);
                if (currentIndex == -1)
                {
                    break;
                }
                continue;
            }
            
            var valueEnd = input.IndexOf('-', paramEnd);
            if (valueEnd == -1)
            {
                valueEnd = input.Length;
            }
    
            var paramValue = input.Substring(paramEnd + 1, valueEnd - paramEnd - 1).Trim();

            if (string.IsNullOrWhiteSpace(paramValue))
            {
                currentIndex = input.IndexOf('-', paramEnd + 1);
                continue;
            }
            
            var headersForHeaderType = paramValue
                .Split(',')
                .Select(x => x.Trim())
                .ToArray();
    
            parameters[headerType] = headersForHeaderType;
    
            currentIndex = valueEnd;
        }
    
        return parameters;
    }
}