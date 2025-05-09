using System;
using System.Globalization;

public static class TimeExtensions
{
    /// <summary>
    /// Converts a string to DateTime using DateTime.TryParse.
    /// Returns DateTime.MinValue if parsing fails.
    /// </summary>
    public static DateTime StringToDateTime(this string s)
    {
        if (DateTime.TryParse(s, out DateTime result))
        {
            return result;
        }
        return DateTime.MinValue; // or throw an exception depending on your needs
    }

    /// <summary>
    /// Converts a string to DateTime using a specific format.
    /// Returns DateTime.MinValue if parsing fails.
    /// </summary>
    public static DateTime StringToDateTime(this string s, string format, CultureInfo culture = null)
    {
        culture ??= CultureInfo.InvariantCulture;
        if (DateTime.TryParseExact(s, format, culture, DateTimeStyles.None, out DateTime result))
        {
            return result;
        }
        return DateTime.MinValue; // or throw new FormatException("Invalid date format");
    }
}