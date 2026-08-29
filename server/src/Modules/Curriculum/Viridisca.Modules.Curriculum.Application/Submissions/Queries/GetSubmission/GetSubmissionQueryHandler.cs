using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmission;

internal sealed class GetSubmissionQueryHandler : IRequestHandler<GetSubmissionQuery, SubmissionDto>
{
    private readonly ISubmissionRepository _submissionRepository;

    public GetSubmissionQueryHandler(ISubmissionRepository submissionRepository)
    {
        _submissionRepository = submissionRepository;
    }

    public async Task<SubmissionDto> Handle(GetSubmissionQuery request, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByUidAsync(request.SubmissionUid, cancellationToken)
            ?? throw new InvalidOperationException($"Работа с ID {request.SubmissionUid} не найдена");

        return SubmissionMapper.Map(submission);
    }
}

internal static class SubmissionMapper
{
    public static SubmissionDto Map(Submission submission) => new()
    {
        Uid = submission.Uid,
        AssignmentUid = submission.AssignmentUid,
        StudentUid = submission.StudentUid,
        Content = submission.Content,
        FilePath = submission.FilePath,
        SubmissionDate = submission.SubmissionDate,
        Status = submission.Status.ToString(),
        Score = submission.Score,
        Feedback = submission.Feedback,
        GradedByUid = submission.GradedByUid,
        GradedAtUtc = submission.GradedAtUtc,
        CreatedAtUtc = submission.CreatedAtUtc,
    };
}
