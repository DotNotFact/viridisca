using FluentValidation;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Commands.CreateCourseInstance;

public sealed class CreateCourseInstanceCommandValidator : AbstractValidator<CreateCourseInstanceCommand>
{
    public CreateCourseInstanceCommandValidator()
    {
        RuleFor(x => x.SubjectUid).NotEmpty().WithMessage("ID предмета обязателен");
        RuleFor(x => x.GroupUid).NotEmpty().WithMessage("ID группы обязателен");
        RuleFor(x => x.AcademicPeriodUid).NotEmpty().WithMessage("ID учебного периода обязателен");
        RuleFor(x => x.Name).NotEmpty().WithMessage("Название обязательно");
        RuleFor(x => x.MaxEnrollments).GreaterThan(0).WithMessage("Максимальное число студентов должно быть положительным");
    }
}
