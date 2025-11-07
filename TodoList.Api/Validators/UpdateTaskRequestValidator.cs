using FluentValidation;
using TodoList.Api.Dtos;

namespace TodoList.Api.Validators;

/// <summary>
/// Validator for UpdateTaskRequest using FluentValidation
/// No async database validation to avoid performance issues
/// </summary>
public class UpdateTaskRequestValidator : AbstractValidator<TaskItemUpdateDto>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(3, 100)
            .WithMessage("Title must be between 3 and 100 characters");
    }
}