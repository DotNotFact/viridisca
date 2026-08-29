using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.UnpublishGrade;

internal sealed class UnpublishGradeCommandHandler : IRequestHandler<UnpublishGradeCommand, bool>
{
    private readonly IGradeRepository _gradeRepository;

    public UnpublishGradeCommandHandler(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    public async Task<bool> Handle(UnpublishGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _gradeRepository.GetByUidAsync(request.GradeUid, cancellationToken)
            ?? throw new InvalidOperationException($"Grade with UID {request.GradeUid} not found");

        grade.Unpublish();
        await _gradeRepository.UpdateGradeAsync(grade, cancellationToken);

        return true;
    }
}
