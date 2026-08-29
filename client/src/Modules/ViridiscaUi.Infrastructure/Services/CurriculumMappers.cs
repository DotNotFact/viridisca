using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Maps backend Curriculum API DTOs to the client's own (richer) domain entities.
/// All backend enum member names are a subset of the client's own enums here (unlike
/// SubjectType in the Academic module), so plain Enum.Parse is safe throughout.
/// </summary>
internal static class CurriculumMappers
{
    public static AcademicPeriod ToAcademicPeriod(AcademicPeriodResponseDto dto) => new()
    {
        Uid = dto.Uid,
        Name = dto.Name,
        Code = dto.Code,
        Description = dto.Description,
        Type = Enum.Parse<AcademicPeriodType>(dto.Type),
        Status = Enum.Parse<AcademicPeriodStatus>(dto.Status),
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        AcademicYear = dto.AcademicYear,
        IsCurrent = dto.IsCurrent,
        IsActive = true,
    };

    public static CourseInstance ToCourseInstance(CourseInstanceResponseDto dto) => new()
    {
        Uid = dto.Uid,
        SubjectUid = dto.SubjectUid,
        GroupUid = dto.GroupUid,
        AcademicPeriodUid = dto.AcademicPeriodUid,
        TeacherUid = dto.TeacherUid,
        Name = dto.Name,
        Code = dto.Code,
        Description = dto.Description,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        MaxEnrollments = dto.MaxEnrollments,
        Status = Enum.Parse<CourseStatus>(dto.Status),
        IsActive = true,
    };

    public static Assignment ToAssignment(AssignmentResponseDto dto) => new()
    {
        Uid = dto.Uid,
        CourseInstanceUid = dto.CourseInstanceUid,
        Title = dto.Title,
        Description = dto.Description,
        Instructions = dto.Instructions,
        DueDate = dto.DueDate,
        MaxScore = (double)dto.MaxScore,
        Type = Enum.Parse<AssignmentType>(dto.Type),
        Difficulty = Enum.Parse<AssignmentDifficulty>(dto.Difficulty),
        Status = Enum.Parse<AssignmentStatus>(dto.Status),
        IsPublished = dto.IsPublished,
    };

    public static Submission ToSubmission(SubmissionResponseDto dto) => new()
    {
        Uid = dto.Uid,
        AssignmentUid = dto.AssignmentUid,
        StudentUid = dto.StudentUid,
        Content = dto.Content,
        FilePath = dto.FilePath,
        SubmissionDate = dto.SubmissionDate,
        Status = Enum.Parse<SubmissionStatus>(dto.Status),
        Score = dto.Score.HasValue ? (double)dto.Score.Value : null,
        Feedback = dto.Feedback,
        GradedByUid = dto.GradedByUid,
        GradedDate = dto.GradedAtUtc,
        IsActive = true,
    };
}
