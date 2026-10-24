using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using SkillScape.Domain.Entities;
using SkillScape.Infrastructure.Services;
using Xunit;

namespace SkillScape.Tests;

public class ExploreQuestionFilteringTests
{
    [Theory]
    [InlineData("MBBS", "Medicine", "Healthcare", false)]
    [InlineData("B.Tech", "Computer Science", "Technology", false)]
    [InlineData("LLB", "Law", "Law", false)]
    [InlineData("B.Com", "Commerce", "Commerce", false)]
    [InlineData("BSc", "Biology", "Science", false)]
    [InlineData("B.Ed", "Education", "Education", false)]
    [InlineData("B.Des", "Fashion Design", "Design", true)]
    public void ExploreQuiz_RejectsDesignRootOutsideLearnersField(
        string degree, string major, string eligibleDomain, bool expectedAllowed)
    {
        var validator = new ProfileValidationEngine(NullLogger<ProfileValidationEngine>.Instance);
        var context = new AssessmentContext
        {
            EducationLevel = "AfterGraduation",
            Degree = degree,
            Major = major,
            PreferenceMode = "EXPLORE",
            EligibleDomains = new List<string> { eligibleDomain }
        };
        var question = new QuizQuestionV2
        {
            Id = "FB_GEN_DESIGN_ROOT",
            PhaseKey = "AfterGraduation",
            Domain = "Universal",
            TargetDiscriminatesJson = "[\"design\"]"
        };

        var allowed = validator.IsQuestionAllowed(question, context, new List<string>(), out _);

        Assert.Equal(expectedAllowed, allowed);
    }
}
