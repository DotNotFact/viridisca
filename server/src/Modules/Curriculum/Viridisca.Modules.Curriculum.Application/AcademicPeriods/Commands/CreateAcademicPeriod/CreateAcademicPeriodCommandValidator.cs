using FluentValidation;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Commands.CreateAcademicPeriod;

public sealed class CreateAcademicPeriodCommandValidator : AbstractValidator<CreateAcademicPeriodCommand>
{
    public CreateAcademicPeriodCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Название периода обязательно");
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20).WithMessage("Код периода обязателен и не длиннее 20 символов");
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("Дата окончания должна быть позже даты начала");
    }
}
