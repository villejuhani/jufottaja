namespace Jufottaja.Tests;

[TestFixture]
public class ConsoleHelperTests
{
    [Test]
    public void ExtractParameters_ValidInput_ParsesCorrectly()
    {
        // Arrange
        const string input = "-name:Name,Title -isbn:Publication ISBN -issn: ISSN ";

        // Act
        var result = InvokeExtractParameters(input);

        // Assert
        Assert.That(result.ContainsKey(HeaderType.Name), Is.True);
        Assert.That(result[HeaderType.Name], Is.EqualTo(new[] { "Name", "Title" }));
        Assert.That(result.ContainsKey(HeaderType.Isbn), Is.True);
        Assert.That(result[HeaderType.Isbn], Is.EqualTo(new[] { "Publication ISBN" }));
        Assert.That(result.ContainsKey(HeaderType.Issn), Is.True);
        Assert.That(result[HeaderType.Issn], Is.EqualTo(new[] { "ISSN" }));
    }

    [Test]
    public void ExtractParameters_InvalidHeaderType_SkipsUnknownParameter()
    {
        // Arrange
        const string input = "-invalid:Value -name:Name";


        // Act
        var result = InvokeExtractParameters(input);

        // Assert
        Assert.That(result.ContainsKey((HeaderType)(-1)), Is.False); // Invalid
        Assert.That(result.ContainsKey(HeaderType.Name), Is.True);
        Assert.That(result[HeaderType.Name], Is.EqualTo(new[] { "Name" }));
    }

    [Test]
    public void ExtractParameters_EmptyValues_SkipsEmptyParameters()
    {
        // Arrange
        const string input = "-name: -isbn:ISBN";

        // Act
        var result = InvokeExtractParameters(input);

        // Assert
        Assert.That(result.ContainsKey(HeaderType.Name), Is.False);
        Assert.That(result.ContainsKey(HeaderType.Isbn), Is.True);
        Assert.That(result[HeaderType.Isbn], Is.EqualTo(new[] { "ISBN" }));
    }

    [Test]
    public void ExtractParameters_MissingColon_SkipsIncorrectlyFormattedHeaders()
    {
        // Arrange
        const string input = "-name,Title -isbn:ISBN -issn:ISSN";


        // Act
        var result = InvokeExtractParameters(input);

        // Assert
        Assert.That(result.ContainsKey(HeaderType.Name), Is.False);
        Assert.That(result.ContainsKey(HeaderType.Isbn), Is.False);
        Assert.That(result.ContainsKey(HeaderType.Issn), Is.True);
        Assert.That(result[HeaderType.Issn], Is.EqualTo(new[] { "ISSN" }));
    }

    private static Dictionary<HeaderType, string[]> InvokeExtractParameters(string input)
    {
        // Directly call the private ExtractParameters method using reflection
        var method = typeof(ConsoleHelper).GetMethod("ExtractParameters",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (Dictionary<HeaderType, string[]>)method.Invoke(null, new object[] { input });
    }
}