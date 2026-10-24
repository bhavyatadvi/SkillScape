using FluentValidation;
using SkillScape.Application.DTOs;

namespace SkillScape.Application.Validation;

/// <summary>
/// Validators for request DTOs
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MinimumLength(3).WithMessage("Name must be at least 3 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match");

        RuleFor(x => x.Role)
            .Must(x => x == "Student")
            .WithMessage("Only Student accounts can be registered");
    }
}

public class SubmitQuizRequestValidator : AbstractValidator<SubmitQuizRequest>
{
    public SubmitQuizRequestValidator()
    {
        RuleFor(x => x.Responses)
            .NotNull().WithMessage("Quiz responses are required")
            .Must(responses => responses != null && responses.Count >= SubmitQuizRequest.MinimumRequiredQuestions)
            .WithMessage($"At least {SubmitQuizRequest.MinimumRequiredQuestions} quiz responses are required.");
    }
}

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name cannot exceed 100 characters");
            
        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio cannot exceed 500 characters");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters")
            .Equal(x => x.ConfirmPassword).WithMessage("Passwords do not match");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}

public class SaveCareerProfileRequestValidator : AbstractValidator<SaveCareerProfileRequest>
{
    public SaveCareerProfileRequestValidator()
    {
        RuleFor(x => x.EducationLevel).MaximumLength(100);
        RuleFor(x => x.Degree).MaximumLength(200);
        RuleFor(x => x.Institution).MaximumLength(200);
        RuleFor(x => x.CurrentOccupation).MaximumLength(200);
    }
}

public class SkillAnalyzerRequestValidator : AbstractValidator<SkillAnalyzerRequest>
{
    public SkillAnalyzerRequestValidator()
    {
        RuleFor(x => x.PreparationType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DomainId).NotEmpty().MaximumLength(100);
    }
}

public class AskAIRoadmapRequestDtoValidator : AbstractValidator<AskAIRoadmapRequestDto>
{
    public AskAIRoadmapRequestDtoValidator()
    {
        RuleFor(x => x.GoalText).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SituationText).MaximumLength(1000);
    }
}

public class QuizSubmissionRequestValidator : AbstractValidator<QuizSubmissionRequest>
{
    public QuizSubmissionRequestValidator()
    {
        RuleFor(x => x.SelectedStream).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Answers).NotEmpty().WithMessage("Quiz answers are required");
    }
}

public class SalaryForecastRequestValidator : AbstractValidator<SalaryForecastRequest>
{
    public SalaryForecastRequestValidator()
    {
        RuleFor(x => x.CareerPathId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(50);
        RuleFor(x => x.SkillLevel).NotEmpty().MaximumLength(50);
    }
}

public class SubmitPlacementRequestValidator : AbstractValidator<SubmitPlacementRequest>
{
    public SubmitPlacementRequestValidator()
    {
        RuleFor(x => x.TargetRole).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Answers).NotEmpty().WithMessage("Placement answers are required");
    }
}

public class AiPromptMessageRequestValidator : AbstractValidator<AiPromptMessageRequest>
{
    public AiPromptMessageRequestValidator()
    {
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public class AiQuizGenerationRequestValidator : AbstractValidator<AiQuizGenerationRequest>
{
    public AiQuizGenerationRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
    }
}

public class AiCareerPredictionRequestValidator : AbstractValidator<AiCareerPredictionRequest>
{
    public AiCareerPredictionRequestValidator()
    {
        RuleFor(x => x.Category).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Answers).NotEmpty().Must(x => x.Count > 0);
    }
}
