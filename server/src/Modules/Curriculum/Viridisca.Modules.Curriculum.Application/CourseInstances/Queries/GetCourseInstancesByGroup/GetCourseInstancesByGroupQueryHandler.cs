using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstance;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstancesByGroup;

internal sealed class GetCourseInstancesByGroupQueryHandler : IRequestHandler<GetCourseInstancesByGroupQuery, List<CourseInstanceDto>>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;

    public GetCourseInstancesByGroupQueryHandler(ICourseInstanceRepository courseInstanceRepository)
    {
        _courseInstanceRepository = courseInstanceRepository;
    }

    public async Task<List<CourseInstanceDto>> Handle(GetCourseInstancesByGroupQuery request, CancellationToken cancellationToken)
    {
        var courseInstances = await _courseInstanceRepository.GetByGroupUidAsync(request.GroupUid, cancellationToken);
        return courseInstances.Select(CourseInstanceMapper.Map).ToList();
    }
}
