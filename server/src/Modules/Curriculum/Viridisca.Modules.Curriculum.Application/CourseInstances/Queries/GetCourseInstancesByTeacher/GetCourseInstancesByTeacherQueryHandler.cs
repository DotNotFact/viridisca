using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstance;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstancesByTeacher;

internal sealed class GetCourseInstancesByTeacherQueryHandler : IRequestHandler<GetCourseInstancesByTeacherQuery, List<CourseInstanceDto>>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;

    public GetCourseInstancesByTeacherQueryHandler(ICourseInstanceRepository courseInstanceRepository)
    {
        _courseInstanceRepository = courseInstanceRepository;
    }

    public async Task<List<CourseInstanceDto>> Handle(GetCourseInstancesByTeacherQuery request, CancellationToken cancellationToken)
    {
        var courseInstances = await _courseInstanceRepository.GetByTeacherUidAsync(request.TeacherUid, cancellationToken);
        return courseInstances.Select(CourseInstanceMapper.Map).ToList();
    }
}
