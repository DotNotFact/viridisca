using FluentValidation;

namespace Viridisca.Modules.Academic.Application.Subjects.Commands.CreateSubject;

public sealed class CreateSubjectCommandValidator : AbstractValidator<CreateSubjectCommand>
{
    public CreateSubjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Название предмета обязательно")
            .MaximumLength(256).WithMessage("Название предмета не должно превышать 256 символов");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Код предмета обязателен")
            .MaximumLength(20).WithMessage("Код предмета не должен превышать 20 символов");

        RuleFor(x => x.Credits)
            .GreaterThan(0).WithMessage("Количество кредитов должно быть положительным");
    }
}
