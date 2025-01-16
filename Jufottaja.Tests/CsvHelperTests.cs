namespace Jufottaja.Tests;

[TestFixture]
public class CsvHelperTests
{
    [Test]
        public void GetHeaderTypesAndFieldIndexes_AllHeadersPresent_ReturnsCorrectIndexes()
        {
            // Arrange
            var fileHeaders = new[] { "Name", "Isbn", "Issn", "ConferenceAbbreviation" };
            var headers = new Dictionary<HeaderType, string[]>
            {
                { HeaderType.Name, ["Name"] },
                { HeaderType.Isbn, ["Isbn"] },
                { HeaderType.Issn, ["Issn"] },
                { HeaderType.ConferenceAbbreviation, ["ConferenceAbbreviation"] }
            };

            // Act
            var result = CsvHelper.GetHeaderTypesAndFieldIndexes(fileHeaders, headers);

            // Assert
            Assert.That(result[HeaderType.Name], Has.Count.EqualTo(1));
            Assert.That(result[HeaderType.Name][0], Is.EqualTo(0));
            Assert.That(result[HeaderType.Isbn], Has.Count.EqualTo(1));
            Assert.That(result[HeaderType.Isbn][0], Is.EqualTo(1));
            Assert.That(result[HeaderType.Issn], Has.Count.EqualTo(1));
            Assert.That(result[HeaderType.Issn][0], Is.EqualTo(2));
            Assert.That(result[HeaderType.ConferenceAbbreviation], Has.Count.EqualTo(1));
            Assert.That(result[HeaderType.ConferenceAbbreviation][0], Is.EqualTo(3));
        }

        [Test]
        public void GetHeaderTypesAndFieldIndexes_SomeHeadersMissing_ReturnsOnlyExistingHeaders()
        {
            // Arrange
            var fileHeaders = new[] { "Name", "ConferenceAbbreviation" };
            var headers = new Dictionary<HeaderType, string[]>
            {
                { HeaderType.Name, ["Name"] },
                { HeaderType.Isbn, ["Isbn"] },
                { HeaderType.ConferenceAbbreviation, ["ConferenceAbbreviation"] }
            };

            // Act
            var result = CsvHelper.GetHeaderTypesAndFieldIndexes(fileHeaders, headers);

            // Assert
            Assert.That(result[HeaderType.Name], Has.Count.EqualTo(1));
            Assert.That(result[HeaderType.Name][0], Is.EqualTo(0));
            Assert.That(result[HeaderType.Isbn], Is.Empty); // Missing header
            Assert.That(result[HeaderType.ConferenceAbbreviation], Has.Count.EqualTo(1));
            Assert.That(result[HeaderType.ConferenceAbbreviation][0], Is.EqualTo(1));
        }

        [Test]
        public void GetHeaderTypesAndFieldIndexes_NoHeaders_ReturnsEmptyDictionary()
        {
            // Arrange
            var fileHeaders = Array.Empty<string>();
            var headers = new Dictionary<HeaderType, string[]>
            {
                { HeaderType.Name, ["Name"] }
            };

            // Act
            var result = CsvHelper.GetHeaderTypesAndFieldIndexes(fileHeaders, headers);

            // Assert
            Assert.That(result[HeaderType.Name], Is.Empty);
        }

        [Test]
        public void GetHeaderTypesAndFieldIndexes_EmptyHeaders_ReturnsEmptyIndexes()
        {
            // Arrange
            var fileHeaders = new[] { "Name", "Type" };
            var headers = new Dictionary<HeaderType, string[]>();

            // Act
            var result = CsvHelper.GetHeaderTypesAndFieldIndexes(fileHeaders, headers);

            // Assert
            Assert.That(result, Is.Empty);
        }
    
    [Test]
    public void GetFieldIndexes_ReturnsCorrectIndexes_WhenFieldsMatchValues()
    {
        // Arrange
        string[] fields = ["Title", "Author", "name of publication", "Year"];
        IEnumerable<string> values = new[] { "name of publication", "Title" };

        // Act
        var result = CsvHelper.GetFieldIndexes(fields, values);

        // Assert
        Assert.That(result, Is.EqualTo(new List<int> { 2, 0 }));
    }

    [Test]
    public void GetFieldIndexes_ReturnsEmptyList_WhenNoFieldsMatchValues()
    {
        // Arrange
        string[] fields = ["Title", "Author", "Publisher", "Year"];
        IEnumerable<string> values = new[] { "ISBN", "Editor" };

        // Act
        var result = CsvHelper.GetFieldIndexes(fields, values);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetFieldIndexes_ReturnsEmptyList_WhenValuesIsEmpty()
    {
        // Arrange
        string[] fields = ["Title", "Author", "Publisher", "Year"];
        IEnumerable<string> values = Array.Empty<string>();

        // Act
        var result = CsvHelper.GetFieldIndexes(fields, values);

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void EscapeCsvField_AddsQuotes_WhenFieldContainsComma()
    {
        // Arrange
        const string field = "Author, Publisher";

        // Act
        var result = CsvHelper.EscapeCsvField(field);

        // Assert
        Assert.That(result, Is.EqualTo("\"Author, Publisher\""));
    }

    [Test]
    public void EscapeCsvField_AddsQuotes_WhenFieldContainsDoubleQuotes()
    {
        // Arrange
        const string field = "Title \"Subtitle\"";

        // Act
        var result = CsvHelper.EscapeCsvField(field);

        // Assert
        Assert.That(result, Is.EqualTo("\"Title \"\"Subtitle\"\"\""));
    }

    [Test]
    public void EscapeCsvField_AddsQuotes_WhenFieldContainsNewline()
    {
        // Arrange
        const string field = "Author\nPublisher";

        // Act
        var result = CsvHelper.EscapeCsvField(field);

        // Assert
        Assert.That(result, Is.EqualTo("\"Author\nPublisher\""));
    }

    [Test]
    public void EscapeCsvField_DoesNotAddQuotes_WhenFieldIsPlainText()
    {
        // Arrange
        const string field = "Title";

        // Act
        var result = CsvHelper.EscapeCsvField(field);

        // Assert
        Assert.That(result, Is.EqualTo("Title"));
    }
}