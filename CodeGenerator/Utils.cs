using System.Text.RegularExpressions;

namespace CodeGenerator;

public static class Utils
{
    public static string ToLowerFirstChar(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLower(input[0]) + input.Substring(1);
    }

    public static string ToParamCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var hyphenated = Regex.Replace(input, @"([a-z])([A-Z])", "$1-$2");
        return hyphenated.ToLower();
    }

    public static string ToSentenceCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var hyphenated = Regex.Replace(input, @"([a-z])([A-Z])", "$1 $2");
        return hyphenated;
    }
}