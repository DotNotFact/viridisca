using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.PublishGrade;

internal sealed class PublishGradeCommandHandler : IRequestHandler<PublishGradeCommand, bool>
{
    private readonly IGradeRepository _gradeRepository;

    public PublishGradeCommandHandler(IGradeRepository gradeRepository)
    {
        _gradeRepository = gradeRepository;
    }

    public async Task<bool> Handle(PublishGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _gradeRepository.GetByUidAsync(request.GradeUid, cancellationToken)
            ?? throw new InvalidOperationException($"Grade with UID {request.GradeUid} not found");

        grade.Publish();
        await _gradeRepository.UpdateGradeAsync(grade, cancellationToken);

        return true;
    }
}
