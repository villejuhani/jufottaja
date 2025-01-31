using Jufottaja;
using Microsoft.VisualBasic.FileIO;

class Program
{
    private static async Task Main(string[] args)
    {
        var width = Console.WindowWidth;

        if (args.Length == 0)
        {
            Console.WriteLine("Give the .csv filepath you want to process as a parameter.");
            Console.WriteLine(@"Example usage: .\Jufottaja.exe C:\sources.csv");
            return;
        }

        var filePath = args[0];
        var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath)!, "jufotettu.csv");

        var headers = ConsoleHelper.GetHeaders();

        Console.WriteLine(new string('-', width));
        await ProcessFile(filePath, outputFilePath, headers, width);

        Console.WriteLine(new string('-', width));
        Console.WriteLine($"Copy of your file with a \"Jufo\" level column has been added in {outputFilePath}");
        Console.WriteLine("Happy researching ^_^");
    }

    private static async Task ProcessFile(string filePath, string outputFilePath, Dictionary<HeaderType, string[]> headers, int width)
    {
        Console.WriteLine($"Processing file {filePath}...");

        var rows = 0;
        var failedApiCalls = new List<JufoApiQueryParameters>();
        var addedJufoLevels = 0;
        var noJufoLevel = 0;
        var emptyHeaderColumnRows = 0;
        var multipleJufoChannelPossibilites = 0;

        try
        {
            using var parser = new TextFieldParser(filePath);
            parser.TextFieldType = FieldType.Delimited;
            parser.SetDelimiters(",");
            parser.HasFieldsEnclosedInQuotes = true;

            await using var writer = new StreamWriter(outputFilePath);
            var apiClient = new ApiClient(new HttpClient());

            // Process headers
            var fileHeaders = parser.ReadFields();
            if (fileHeaders == null)
            {
                Console.WriteLine($"Could not find specified headers on the first line of the file.");
                return;
            }

            var headerIndexes = CsvHelper.GetHeaderTypesAndFieldIndexes(fileHeaders, headers);

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

                var jufoApiQueryParameters = new JufoApiQueryParameters();

                // Go through each headertype and it's columns.
                // Set value to the corresponding JufoApiQueryParameters property from the headertype's first column index in the file that is not empty.   
                foreach (var kvp in headerIndexes)
                {
                    var headerType = kvp.Key;
                    var columnIndexes = kvp.Value;

                    var headerValue = CsvHelper.GetFirstOrDefaultInFieldsFromIndexes(columnIndexes, fields);

                    if (string.IsNullOrEmpty(headerValue))
                    {
                        continue;
                    }

                    // Get the property info for the current header type (e.g., Name, Isbn, etc.)
                    var propertyInfo = jufoApiQueryParameters.GetType().GetProperty(headerType.ToString());

                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(jufoApiQueryParameters, headerValue);
                    }
                }

                if (string.IsNullOrEmpty(jufoApiQueryParameters.Name)
                    && string.IsNullOrEmpty(jufoApiQueryParameters.Isbn)
                    && string.IsNullOrEmpty(jufoApiQueryParameters.Issn)
                    && string.IsNullOrEmpty(jufoApiQueryParameters.ConferenceAbbreviation))
                {
                    rows++;
                    emptyHeaderColumnRows++;
                    writer.WriteLine(string.Join(",", fields.Select(CsvHelper.EscapeCsvField)));
                    Console.WriteLine($"Processed row {rows}");
                    continue;
                }

                try
                {
                    var channelId = await apiClient.GetJufoChannelId(jufoApiQueryParameters);

                    if (string.IsNullOrEmpty(channelId))
                    {
                        rows++;
                        failedApiCalls.Add(jufoApiQueryParameters);
                        writer.WriteLine(string.Join(",", fields.Select(CsvHelper.EscapeCsvField)));
                        Console.WriteLine($"Processed row {rows}");
                        continue;
                    }

                    if (string.Equals(channelId, "MULTIPLE RESULTS"))
                    {
                        rows++;
                        multipleJufoChannelPossibilites++;
                        var fieldsAndNoLevel = new List<string>(fields) { "MULTIPLE POSSIBILITIES" };
                        writer.WriteLine(string.Join(",", fieldsAndNoLevel.Select(CsvHelper.EscapeCsvField)));
                        Console.WriteLine($"Processed row {rows}");
                        continue;
                    }

                    if (string.Equals(channelId, "NO RESULT"))
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
                    failedApiCalls.Add(jufoApiQueryParameters);
                }

                rows++;
                Console.WriteLine($"Processed row {rows}");
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains("The process cannot access the file"))
            {
                Console.WriteLine(
                    $"The file {outputFilePath} is currently being used by another process. Please close any programs using it and try again.");
                Environment.Exit(1);
            }

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
        Console.WriteLine(
            $"  - Multiple possible channels found in Jufo: {multipleJufoChannelPossibilites} (look the correct one up manually)");
        Console.WriteLine(
            $"  - Rows with all empty {string.Join(", ", headers.SelectMany(kvp => kvp.Value))} column(s): {emptyHeaderColumnRows}");
        Console.WriteLine($"  - Failed to add Jufo level: {failedApiCalls.Count}");

        if (failedApiCalls.Count != 0)
        {
            foreach (var failedApiCall in failedApiCalls)
            {
                Console.WriteLine($"    - Failed to add Jufo level for: {failedApiCall}");
            }
        }
    }
}