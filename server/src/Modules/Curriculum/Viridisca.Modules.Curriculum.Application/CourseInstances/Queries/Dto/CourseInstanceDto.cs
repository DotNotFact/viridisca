using System;

namespace Viridisca.Modules.Curriculum.Application.CourseInstances.Queries.Dto;

public sealed class CourseInstanceDto
{
    public Guid Uid { get; set; }
    public Guid SubjectUid { get; set; }
    public Guid GroupUid { get; set; }
    public Guid AcademicPeriodUid { get; set; }
    public Guid? TeacherUid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxEnrollments { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
