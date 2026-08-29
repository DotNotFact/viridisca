using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmission;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmissionsByAssignment;

internal sealed class GetSubmissionsByAssignmentQueryHandler : IRequestHandler<GetSubmissionsByAssignmentQuery, List<SubmissionDto>>
{
    private readonly ISubmissionRepository _submissionRepository;

    public GetSubmissionsByAssignmentQueryHandler(ISubmissionRepository submissionRepository)
    {
        _submissionRepository = submissionRepository;
    }

    public async Task<List<SubmissionDto>> Handle(GetSubmissionsByAssignmentQuery request, CancellationToken cancellationToken)
    {
        var submissions = await _submissionRepository.GetByAssignmentUidAsync(request.AssignmentUid, cancellationToken);
        return submissions.Select(SubmissionMapper.Map).ToList();
    }
}
