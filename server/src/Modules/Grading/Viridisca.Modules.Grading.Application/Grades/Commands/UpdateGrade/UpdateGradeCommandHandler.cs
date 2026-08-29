using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Grading.Domain.Models;
using Viridisca.Modules.Grading.Domain.Repositories;

namespace Viridisca.Modules.Grading.Application.Grades.Commands.UpdateGrade;

internal sealed class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand, bool>
{
    private readonly IGradeRepository _gradeRepository;
    private readonly IGradeRevisionsRepository _gradeRevisionsRepository;

    public UpdateGradeCommandHandler(IGradeRepository gradeRepository, IGradeRevisionsRepository gradeRevisionsRepository)
    {
        _gradeRepository = gradeRepository;
        _gradeRevisionsRepository = gradeRevisionsRepository;
    }

    public async Task<bool> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
    {
        var grade = await _gradeRepository.GetByUidAsync(request.GradeUid, cancellationToken)
            ?? throw new InvalidOperationException($"Grade with UID {request.GradeUid} not found");

        var valueChanged = grade.Value != request.Value;

        if (valueChanged)
        {
            grade.UpdateValue(request.Value, request.Reason);
        }

        if (request.Description is not null)
        {
            grade.UpdateDescription(request.Description);
        }

        if (valueChanged)
        {
            // Grade.UpdateValue() adds the new GradeRevision only to the entity's own
            // in-memory collection — EF doesn't reliably pick that up as an insert once the
            // parent is already tracked (same class of bug fixed for RefreshToken/
            // TeacherSubject/StudentParent earlier in this project). Track it explicitly,
            // and do it *before* saving the grade itself so it's already Added by the time
            // the graph gets walked, instead of ambiguously Modified.
            var newRevision = grade.Revisions.OrderByDescending(r => r.CreatedAtUtc).First();
            await _gradeRevisionsRepository.AddRevisionAsync(newRevision, cancellationToken);
        }

        await _gradeRepository.UpdateGradeAsync(grade, cancellationToken);

        return true;
    }
}
