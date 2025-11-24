using System.Globalization;

namespace Infrastructure.Extensions;

static public class IntExtensions
{
    static public string ToStr(this int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
    
    static public int TryPrs(this string? value, int dflt = -1)
    {
        var success = int.TryParse(value, CultureInfo.InvariantCulture, out var valueInt);
        return success ? valueInt : dflt;
    }
}
