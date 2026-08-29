using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Common.Application.Data;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Commands.GradeSubmission;

internal sealed class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, bool>
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GradeSubmissionCommandHandler(ISubmissionRepository submissionRepository, IUnitOfWork unitOfWork)
    {
        _submissionRepository = submissionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByUidAsync(request.SubmissionUid, cancellationToken)
            ?? throw new InvalidOperationException($"Работа с ID {request.SubmissionUid} не найдена");

        var result = submission.Grade(request.Score, request.Feedback, request.GradedByUid);
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error.Message);
        }

        _submissionRepository.Update(submission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
