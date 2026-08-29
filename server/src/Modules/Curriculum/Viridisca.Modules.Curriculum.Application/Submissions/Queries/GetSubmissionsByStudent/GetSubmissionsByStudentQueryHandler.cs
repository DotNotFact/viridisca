using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmission;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.GetSubmissionsByStudent;

internal sealed class GetSubmissionsByStudentQueryHandler : IRequestHandler<GetSubmissionsByStudentQuery, List<SubmissionDto>>
{
    private readonly ISubmissionRepository _submissionRepository;

    public GetSubmissionsByStudentQueryHandler(ISubmissionRepository submissionRepository)
    {
        _submissionRepository = submissionRepository;
    }

    public async Task<List<SubmissionDto>> Handle(GetSubmissionsByStudentQuery request, CancellationToken cancellationToken)
    {
        var submissions = await _submissionRepository.GetByStudentUidAsync(request.StudentUid, cancellationToken);
        return submissions.Select(SubmissionMapper.Map).ToList();
    }
}
