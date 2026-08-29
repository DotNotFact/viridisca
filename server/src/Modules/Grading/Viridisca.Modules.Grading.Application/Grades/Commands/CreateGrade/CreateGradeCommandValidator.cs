using FluentValidation;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.CreateGrade;

public sealed class CreateGradeCommandValidator : AbstractValidator<CreateGradeCommand>
{
    public CreateGradeCommandValidator()
    {
        RuleFor(x => x.StudentUid).NotEmpty().WithMessage("ID студента обязателен");
        RuleFor(x => x.SubjectUid).NotEmpty().WithMessage("ID предмета обязателен");
        RuleFor(x => x.TeacherUid).NotEmpty().WithMessage("ID преподавателя обязателен");
        RuleFor(x => x.Value).InclusiveBetween(0, 100).WithMessage("Значение оценки должно быть от 0 до 100");
    }
}
