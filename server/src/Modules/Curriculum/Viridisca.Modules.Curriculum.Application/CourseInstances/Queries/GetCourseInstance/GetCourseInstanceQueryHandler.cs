using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;
using Viridisca.Modules.Curriculum.Domain.Models;
using Viridisca.Modules.Curriculum.Domain.Repositories;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.GetCourseInstance;

internal sealed class GetCourseInstanceQueryHandler : IRequestHandler<GetCourseInstanceQuery, CourseInstanceDto>
{
    private readonly ICourseInstanceRepository _courseInstanceRepository;

    public GetCourseInstanceQueryHandler(ICourseInstanceRepository courseInstanceRepository)
    {
        _courseInstanceRepository = courseInstanceRepository;
    }

    public async Task<CourseInstanceDto> Handle(GetCourseInstanceQuery request, CancellationToken cancellationToken)
    {
        var courseInstance = await _courseInstanceRepository.GetByUidAsync(request.CourseInstanceUid, cancellationToken)
            ?? throw new InvalidOperationException($"Экземпляр курса с ID {request.CourseInstanceUid} не найден");

        return CourseInstanceMapper.Map(courseInstance);
    }
}

internal static class CourseInstanceMapper
{
    public static CourseInstanceDto Map(CourseInstance courseInstance) => new()
    {
        Uid = courseInstance.Uid,
        SubjectUid = courseInstance.SubjectUid,
        GroupUid = courseInstance.GroupUid,
        AcademicPeriodUid = courseInstance.AcademicPeriodUid,
        TeacherUid = courseInstance.TeacherUid,
        Name = courseInstance.Name,
        Code = courseInstance.Code,
        Description = courseInstance.Description,
        StartDate = courseInstance.StartDate,
        EndDate = courseInstance.EndDate,
        MaxEnrollments = courseInstance.MaxEnrollments,
        Status = courseInstance.Status.ToString(),
        CreatedAtUtc = courseInstance.CreatedAtUtc,
    };
}
