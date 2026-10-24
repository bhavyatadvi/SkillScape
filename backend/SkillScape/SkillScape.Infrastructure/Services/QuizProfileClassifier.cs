using System;
using System.Text.RegularExpressions;

namespace SkillScape.Infrastructure.Services;

internal static class QuizProfileClassifier
{
    private static readonly Regex BachelorOfArtsPrefix = new(
        @"^\s*B\s*\.?\s*A\.?\s*(?:$|[-(/\s])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex BachelorOfSciencePrefix = new(
        @"^\s*B\s*\.?\s*S\s*\.?\s*C\.?\s*(?:$|[-(/\s])",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static bool IsBachelorOfArts(string? degree, string? major)
    {
        return IsBachelorOfArtsLabel(degree) || IsBachelorOfArtsLabel(major);
    }

    public static bool IsBachelorOfScience(string? degree, string? major)
    {
        return IsBachelorOfScienceLabel(degree) || IsBachelorOfScienceLabel(major);
    }

    private static bool IsBachelorOfArtsLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Contains("Bachelor of Arts", StringComparison.OrdinalIgnoreCase)) return true;
        return BachelorOfArtsPrefix.IsMatch(value);
    }

    private static bool IsBachelorOfScienceLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Contains("Bachelor of Science", StringComparison.OrdinalIgnoreCase)) return true;
        return BachelorOfSciencePrefix.IsMatch(value);
    }
}
