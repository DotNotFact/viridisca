using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Maps backend Grading API DTOs to the client's own (richer) Grade entity. Fields the
/// backend doesn't track (CourseInstanceUid, AssignmentUid, ExamUid, Comment, Feedback,
/// MaxValue, Weight) are left at their default — same approach as AcademicMappers.
/// </summary>
internal static class GradingMappers
{
    public static Grade ToGrade(GradeResponseDto dto) => new()
    {
        Uid = dto.Uid,
        StudentUid = dto.StudentUid,
        SubjectUid = dto.SubjectUid,
        TeacherUid = dto.TeacherUid,
        Value = dto.Value,
        Description = dto.Description,
        Type = Enum.TryParse<GradeType>(dto.Type, out var type) ? type : GradeType.Other,
        IssuedAt = dto.IssuedAtUtc,
        IsPublished = dto.IsPublished,
        PublishedAt = dto.PublishedAtUtc,
        IsActive = true,
    };
}
