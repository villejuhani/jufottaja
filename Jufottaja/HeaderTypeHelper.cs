namespace Jufottaja;

public enum HeaderType
{
    Name,
    Isbn,
    Issn,
    ConferenceAbbreviation
}

public static class HeaderTypeHelper
{
    public static bool TryParseHeaderType(string input, out HeaderType headerType)
    {
        switch (input.ToLower())
        {
            case "name":
                headerType = HeaderType.Name;
                return true;
            case "isbn":
                headerType = HeaderType.Isbn;
                return true;
            case "issn":
                headerType = HeaderType.Issn;
                return true;
            case "conferenceAbbreviation":
                headerType = HeaderType.ConferenceAbbreviation;
                return true;
            default:
                headerType = default;
                return false;
        }
    }
}