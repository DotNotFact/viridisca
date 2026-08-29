using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Domain.Models;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.CreateGrade;

internal sealed class CreateGradeCommandHandler : IRequestHandler<CreateGradeCommand, Guid>
{
    private readonly IGradeRepository _gradeRepository;

    public CreateGradeCommandHandler(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    public async Task<Guid> Handle(CreateGradeCommand request, CancellationToken cancellationToken)
    {
        var gradeResult = Grade.Create(
            request.StudentUid,
            request.SubjectUid,
            request.TeacherUid,
            request.Value,
            request.Type,
            request.Description,
            request.LessonUid);

        if (gradeResult.IsFailure)
        {
            throw new InvalidOperationException(gradeResult.Error.Message);
        }

        var grade = gradeResult.Value;

        await _gradeRepository.AddGradeAsync(grade, cancellationToken);

        return grade.Uid;
    }
}
