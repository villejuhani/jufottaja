using System.ComponentModel.DataAnnotations;

namespace Jufottaja;

public class JufoApiQueryParameters
{
    private readonly string? _name;
    
    [MaxLength(50)]
    [RegularExpression("^[a-zA-Z0-9- ]+$", ErrorMessage = "Name must contain only a-Z, 0-9, '-' and spaces")]
    public string? Name
    {
        get => _name;
        init
        {
            if (value?.Length > 50)
            {
                Console.WriteLine($"Truncated name to max allowed length (50): {value}");
                _name = value[..50];
            }
            else
            {
                _name = value;
            }
        }
    }
    
    [MaxLength(20)]
    [RegularExpression("^[0-9\\-]+$", ErrorMessage = "ISBN must contain only numbers 0-9, and '-'")]
    public string? Isbn { get; init; }

    [MaxLength(10)]
    [RegularExpression("^[0-9\\-]+$", ErrorMessage = "ISSN must contain only numbers 0-9, and '-'")]
    public string? Issn { get; init; }
    
    [MaxLength(20)]
    [RegularExpression("^[a-zA-Z0-9- ]+$", ErrorMessage = "Conference abbreviation must contain only a-Z, 0-9, '-' and spaces")]
    public string? ConferenceAbbreviation { get; init; }
    
    public override string ToString()
    {
        var properties = GetType().GetProperties();
        var propertiesWithValue = properties
            .Where(p => p.GetValue(this) != null)
            .Select(p => $"{p.Name}: {p.GetValue(this)}");
        return string.Join(", ", propertiesWithValue);
    }
}