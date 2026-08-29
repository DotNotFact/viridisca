using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstance;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetAllCourseInstances;

internal sealed class GetAllCourseInstancesQueryHandler : IRequestHandler<GetAllCourseInstancesQuery, List<CourseInstanceDto>>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;

    public GetAllCourseInstancesQueryHandler(ICourseInstanceRepository courseInstanceRepository)
    {
        _courseInstanceRepository = courseInstanceRepository;
    }

    public async Task<List<CourseInstanceDto>> Handle(GetAllCourseInstancesQuery request, CancellationToken cancellationToken)
    {
        var courseInstances = await _courseInstanceRepository.GetAllAsync(cancellationToken);
        return courseInstances.Select(CourseInstanceMapper.Map).ToList();
    }
}
