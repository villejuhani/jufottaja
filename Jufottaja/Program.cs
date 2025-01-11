using Jufottaja;
using Microsoft.VisualBasic.FileIO;

if (args.Length == 0)
{
    Console.WriteLine("Give the .csv filepath you want to process as a parameter.");
    Console.WriteLine(@"Example usage: .\Jufottaja.exe C:\sources.csv");
    return;
}

var width = Console.WindowWidth;

var headers = GetHeaders();

var filePath = args[0];
var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, "jufotettu.csv");

Console.WriteLine($"Processing file {filePath}...");

var rows = 0;
var failedApiCalls = new List<string>();
var addedJufoLevels = 0;
var noJufoLevel = 0;
var emptyHeaderColumnRows = 0;

try
{
    using var parser = new TextFieldParser(filePath);
    parser.TextFieldType = FieldType.Delimited;
    parser.SetDelimiters(",");
    parser.HasFieldsEnclosedInQuotes = true;

    await using var writer = new StreamWriter(outputFilePath);
    var apiClient = new ApiClient();
    
    // Process headers
    var fileHeaders = parser.ReadFields();
    if (fileHeaders == null || !fileHeaders.Any(fileHeader => headers.Contains(fileHeader)))
    {
        Console.WriteLine($"Could not find {string.Join(", ", headers)} headers on the first line of the file.");
        return;
    }
    
    var headerIndexes = CsvHelper.GetFieldIndexes(fileHeaders, headers);
    
    var newHeaders = new List<string>(fileHeaders) { "Jufo" };
    writer.WriteLine(string.Join(",", newHeaders.Select(CsvHelper.EscapeCsvField)));
    
    // Process rows
    while (!parser.EndOfData)
    {
        var fields = parser.ReadFields();
        if (fields == null)
        {
            continue; //skip empty line
        }

        var publicationChannelTitle = "";
        foreach (var index in headerIndexes)
        {
            publicationChannelTitle = fields[index];
            if (!string.IsNullOrEmpty(publicationChannelTitle))
            {
                if (publicationChannelTitle.Length > 50)
                {
                    publicationChannelTitle = publicationChannelTitle[..50];
                    Console.WriteLine($"Name is over 50 chars, truncated to: {publicationChannelTitle}");
                }
                break;
            }
        }

        if (string.IsNullOrEmpty(publicationChannelTitle)) 
        {
            rows++;
            emptyHeaderColumnRows++;
            writer.WriteLine(string.Join(",", fields.Select(CsvHelper.EscapeCsvField)));
            Console.WriteLine($"Processed row {rows}");
            continue;
        }
        
        try
        {
            var channelId = await apiClient.GetJufoChannelId(publicationChannelTitle);
            
            if (string.IsNullOrEmpty(channelId))
            {
                rows++;
                noJufoLevel++;
                var fieldsAndNoLevel = new List<string>(fields) { "NO LEVEL" };
                writer.WriteLine(string.Join(",", fieldsAndNoLevel.Select(CsvHelper.EscapeCsvField)));
                Console.WriteLine($"Processed row {rows}");
                continue;
            }
            
            var jufoLevel = await apiClient.GetJufoChannelLevel(channelId);
            
            var fieldsAndJufoLevel = new List<string>(fields) { jufoLevel };
            writer.WriteLine(string.Join(",", fieldsAndJufoLevel.Select(CsvHelper.EscapeCsvField)));
            addedJufoLevels++;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            writer.WriteLine(string.Join(",", fields.Select(CsvHelper.EscapeCsvField)));
            failedApiCalls.Add(publicationChannelTitle);
        }
        
        rows++;
        Console.WriteLine($"Processed row {rows}");
    }
}
catch (Exception e)
{
    Console.WriteLine(e);
    throw;
}

Console.WriteLine(new string('-', width));
Console.WriteLine("File processing finished.");
Console.WriteLine(new string('-', width));

Console.WriteLine("Row Processing Summary:");
Console.WriteLine($"  - Rows Processed: {rows}");
Console.WriteLine($"  - Added Jufo levels: {addedJufoLevels}");
Console.WriteLine($"  - No level found in Jufo: {noJufoLevel} (check these manually to be sure)");
Console.WriteLine($"  - Rows with all empty {string.Join(", ", headers)} column(s): {emptyHeaderColumnRows}");
Console.WriteLine($"  - Failed to add Jufo level: {failedApiCalls.Count}");

if (failedApiCalls.Count != 0)
{
    foreach (var failedApiCall in failedApiCalls)
    {
        Console.WriteLine($"    - Failed to add Jufo level for: {failedApiCall}");
    }
}

Console.WriteLine(new string('-', width));
Console.WriteLine($"Copy of your file with a \"Jufo\" level column has been added in {outputFilePath}");
Console.WriteLine("Happy researching ^_^");
return;

List<string> GetHeaders()
{
    const string defaultHeader = "Publication Title";
    const string defaultHeader2 = "Conference Name";
    
    Console.WriteLine($"Default headers for finding publication channels are \"{defaultHeader}\" and \"{defaultHeader2}\". ");
    Console.WriteLine("To use custom headers write them in comma-delimited list. Example: title,Publisher");
    Console.WriteLine("To continue with the default headers just press enter without writing anything.");
    var input = Console.ReadLine();

    var columnHeaders = new List<string>();
    if (!string.IsNullOrEmpty(input))
    {
        var customHeaders = input.Split(',');
        columnHeaders
            .AddRange(customHeaders
                .Select(header => header.Trim()));
        Console.WriteLine($"Finding publications based on: {string.Join(", ", columnHeaders)}.");
    }
    else
    {
        columnHeaders.Add(defaultHeader);
        columnHeaders.Add(defaultHeader2);
        Console.WriteLine($"Finding publications based: {defaultHeader}, {defaultHeader2}.");
    }

    Console.WriteLine(new string('-', width));
    return columnHeaders;
}