using System.ComponentModel.DataAnnotations;

namespace Jufottaja.Tests;

[TestFixture]
public class JufoApiQueryParametersTests
{
    [Test]
    public void Name_SetWithLengthExceedingMax_ShouldTruncate()
    {
        // Arrange
        var longName = new string('a', 60);

        // Act
        var parameters = new JufoApiQueryParameters
        {
            Name = longName
        };

        // Assert
        Assert.That(parameters.Name.Length, Is.EqualTo(50));
        Assert.That(parameters.Name, Is.EqualTo(new string('a', 50)));
    }

    [Test]
    public void ToString_PropertiesWithValues_ShouldReturnCorrectString()
    {
        // Arrange
        var parameters = new JufoApiQueryParameters
        {
            Name = "TestName",
            Isbn = "1234-5678",
            Issn = null,
            ConferenceAbbreviation = "Conf123"
        };

        // Act
        var result = parameters.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("Name: TestName, Isbn: 1234-5678, ConferenceAbbreviation: Conf123"));
    }

    [Test]
    public void ToString_NoPropertiesWithValues_ShouldReturnEmptyString()
    {
        // Arrange
        var parameters = new JufoApiQueryParameters();

        // Act
        var result = parameters.ToString();

        // Assert
        Assert.That(result, Is.EqualTo(string.Empty));
    }
}