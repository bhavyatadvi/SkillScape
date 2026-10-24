using System.ComponentModel.DataAnnotations;
using SkillScape.Application.DTOs;
using SkillScape.Application.Validation;
using Xunit;

namespace SkillScape.Tests;

public class InputValidationTests
{
    [Fact]
    public void RegisterRequest_RejectsMalformedEmailAndInvalidName()
    {
        var request = new RegisterRequest
        {
            Email = "user@",
            FullName = "A",
            Password = "Password123",
            ConfirmPassword = "Password123",
            Role = "Student"
        };

        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterRequest.Email)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(RegisterRequest.FullName)));
    }

    [Fact]
    public void SubmitQuizRequest_RequiresMinimumResponseCount()
    {
        var request = new SubmitQuizRequest
        {
            PhaseKey = "After12th",
            Responses = Enumerable.Range(0, SubmitQuizRequest.MinimumRequiredQuestions - 1)
                .Select(index => new QuizResponseRequest
                {
                    QuestionId = $"question-{index}",
                    OptionId = $"option-{index}"
                })
                .ToList()
        };

        var results = Validate(request);
        var validatorResult = new SubmitQuizRequestValidator().Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(SubmitQuizRequest.Responses)));
        Assert.False(validatorResult.IsValid);
    }

    [Fact]
    public void CareerGuidanceSubmission_RejectsEmptyAnswers()
    {
        var request = new QuizSubmissionRequest { SelectedStream = "After12th" };

        Assert.Contains(
            Validate(request),
            result => result.MemberNames.Contains(nameof(QuizSubmissionRequest.Answers)));
    }

    [Fact]
    public void AdminQuizQuestion_RequiresTwoOptions()
    {
        var request = new UpsertAdminQuizQuestionDto
        {
            CareerDomainId = "domain-1",
            Text = "Choose an answer",
            Options = new List<AdminQuizOptionCrudDto>
            {
                new() { Text = "Only option" }
            }
        };

        Assert.Contains(
            Validate(request),
            result => result.MemberNames.Contains(nameof(UpsertAdminQuizQuestionDto.Options)));
    }

    [Theory]
    [InlineData("ab", false)]
    [InlineData("abc", true)]
    [InlineData("x", false)]
    public void CreateLearningCircleDto_EnforcesNameLength(string name, bool isValid)
    {
        var request = new CreateLearningCircleDto
        {
            Name = name,
            CareerDomain = "Software Engineering",
            CircleType = "Domain Circle"
        };

        Assert.Equal(isValid, Validate(request).All(result =>
            !result.MemberNames.Contains(nameof(CreateLearningCircleDto.Name))));
    }

    [Fact]
    public void CareerProfileRequest_RejectsOutOfRangeNumericFields()
    {
        var request = new SaveCareerProfileRequest
        {
            GraduationYear = 1959,
            ExperienceYears = 61
        };

        var results = Validate(request);

        Assert.Contains(results, result => result.MemberNames.Contains(nameof(SaveCareerProfileRequest.GraduationYear)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(SaveCareerProfileRequest.ExperienceYears)));
    }

    private static List<ValidationResult> Validate(object instance)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);
        return results;
    }
}
