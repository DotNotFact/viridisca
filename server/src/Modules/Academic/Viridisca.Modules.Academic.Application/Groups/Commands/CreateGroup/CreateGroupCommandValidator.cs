using FluentValidation;

namespace Viridisca.Modules.Academic.Application.Groups.Commands.CreateGroup;

public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Код группы обязателен")
            .MaximumLength(20).WithMessage("Код группы не должен превышать 20 символов");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название группы обязательно")
            .MaximumLength(256).WithMessage("Название группы не должно превышать 256 символов");

        RuleFor(x => x.MaxStudents)
            .GreaterThan(0).WithMessage("Максимальное количество студентов должно быть положительным");

        RuleFor(x => x.DepartmentUid)
            .NotEmpty().WithMessage("Department ID обязателен");
    }
}
