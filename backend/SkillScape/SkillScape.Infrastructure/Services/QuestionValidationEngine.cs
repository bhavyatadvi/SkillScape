using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using SkillScape.Domain.Entities;

namespace SkillScape.Infrastructure.Services;

public interface IQuestionValidationEngine
{
    (bool IsValid, string Reason) ValidateQuestion(QuizQuestionV2 question);
    (bool IsValid, string Reason) ValidateQuestionText(string text);
    (bool IsValid, string Reason) ValidateOptionText(string optionText);
}

public class QuestionValidationEngine : IQuestionValidationEngine
{
    private static readonly Regex PlaceholderRegex = new(
        @"(related\s+to\s+\d+|aspect\s+\d+|\[MOCK|OFFLINE\]|\{.*?\}|undefined|null|threat_detection|subdomain_id|career_id|node_id|_id\b)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public (bool IsValid, string Reason) ValidateQuestion(QuizQuestionV2 question)
    {
        if (question == null)
            return (false, "Question is null.");

        // 1. Validate Question Text
        var textValidation = ValidateQuestionText(question.Text);
        if (!textValidation.IsValid)
            return textValidation;

        // 2. Validate Question Metadata
        if (string.IsNullOrWhiteSpace(question.Id))
            return (false, "Question ID is required.");

        // 3. Validate Options JSON
        if (string.IsNullOrWhiteSpace(question.OptionsJson) || question.OptionsJson == "[]")
            return (false, "Question must have options configured.");

        try
        {
            using var doc = JsonDocument.Parse(question.OptionsJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
                return (false, "OptionsJson must be a JSON array.");

            int optionCount = doc.RootElement.GetArrayLength();
            // Curated career banks include up to eight distinct pathways for
            // fields such as Architecture. Rejecting those questions here made
            // the selector discard nearly the entire specialty bank and fall
            // back to generic prompts.
            if (optionCount < 2 || optionCount > 8)
                return (false, $"Question must have between 2 and 8 options (found {optionCount}).");

            var optionTexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var opt in doc.RootElement.EnumerateArray())
            {
                string text = "";
                if (opt.TryGetProperty("text", out var tProp) || opt.TryGetProperty("Text", out tProp))
                {
                    text = tProp.GetString() ?? "";
                }

                var optValidation = ValidateOptionText(text);
                if (!optValidation.IsValid)
                    return optValidation;

                if (!optionTexts.Add(text.Trim()))
                    return (false, "Duplicate option text detected within the same question.");
            }
        }
        catch (JsonException ex)
        {
            return (false, $"Invalid Options JSON: {ex.Message}");
        }

        return (true, "Valid");
    }

    public (bool IsValid, string Reason) ValidateQuestionText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, "Question text cannot be empty.");

        if (text.Trim().Length < 15)
            return (false, "Question text is too short (minimum 15 characters).");

        if (PlaceholderRegex.IsMatch(text))
            return (false, "Question text contains invalid placeholder, mock tag, or leaked internal variable.");

        return (true, "Valid");
    }

    public (bool IsValid, string Reason) ValidateOptionText(string optionText)
    {
        if (string.IsNullOrWhiteSpace(optionText))
            return (false, "Option text cannot be empty.");

        if (optionText.Trim().Length < 3)
            return (false, "Option text is too short.");

        if (PlaceholderRegex.IsMatch(optionText))
            return (false, "Option text contains invalid placeholder, mock tag, or leaked internal variable.");

        return (true, "Valid");
    }
}
