namespace Jufottaja.Tests;

[TestFixture]
public class CsvHelperTests
{
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