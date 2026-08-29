using FluentValidation;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Commands.CreateAssignment;

public sealed class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(x => x.CourseInstanceUid).NotEmpty().WithMessage("ID экземпляра курса обязателен");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Название задания обязательно");
        RuleFor(x => x.MaxScore).GreaterThan(0).WithMessage("Максимальный балл должен быть положительным");
    }
}
