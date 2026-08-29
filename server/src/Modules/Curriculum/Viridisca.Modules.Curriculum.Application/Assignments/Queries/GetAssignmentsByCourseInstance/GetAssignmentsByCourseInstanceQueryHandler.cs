using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignment;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Queries.GetAssignmentsByCourseInstance;

internal sealed class GetAssignmentsByCourseInstanceQueryHandler : IRequestHandler<GetAssignmentsByCourseInstanceQuery, List<AssignmentDto>>
{
    private readonly IAssignmentRepository _assignmentRepository;

    public GetAssignmentsByCourseInstanceQueryHandler(IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<List<AssignmentDto>> Handle(GetAssignmentsByCourseInstanceQuery request, CancellationToken cancellationToken)
    {
        var assignments = await _assignmentRepository.GetByCourseInstanceUidAsync(request.CourseInstanceUid, cancellationToken);
        return assignments.Select(AssignmentMapper.Map).ToList();
    }
}
