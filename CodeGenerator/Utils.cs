using Fluid.Values;
using Fluid;
using System.Text.RegularExpressions;
using Pluralize.NET;

namespace CodeGenerator;

public static class Utils
{
    private static readonly IPluralize Pluralizer = new Pluralizer();

    public static ValueTask<FluidValue> LowerFirstChar(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        return new StringValue(input.ToStringValue().ToLowerFirstChar());
    }

    public static ValueTask<FluidValue> ParamCase(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        return new StringValue(input.ToStringValue().ToParamCase());
    }

    public static ValueTask<FluidValue> SentenceCase(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        return new StringValue(input.ToStringValue().ToSentenceCase());
    }

    public static ValueTask<FluidValue> Pluralize(FluidValue input, FilterArguments arguments, TemplateContext context)
    {
        return new StringValue(Pluralizer.Pluralize(input.ToStringValue()));
    }

    static string ToLowerFirstChar(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLower(input[0]) + input.Substring(1);
    }

    static string ToParamCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var hyphenated = Regex.Replace(input, @"([a-z])([A-Z])", "$1-$2");
        return hyphenated.ToLower();
    }

    static string ToSentenceCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var hyphenated = Regex.Replace(input, @"([a-z])([A-Z])", "$1 $2");
        return hyphenated;
    }
}