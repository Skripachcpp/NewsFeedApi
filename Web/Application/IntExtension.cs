using System.Globalization;

namespace Web.Application;

static internal class IntExtensions
{
    static public string ToStr(this int value)
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }
}
