using System;

namespace Viridisca.Modules.Grading.Application.Grades.Queries.Dto;

public sealed class GradeDto
{
    public Guid Uid { get; set; }
    public Guid StudentUid { get; set; }
    public Guid SubjectUid { get; set; }
    public Guid TeacherUid { get; set; }
    public Guid? LessonUid { get; set; }
    public decimal Value { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime IssuedAtUtc { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
}
