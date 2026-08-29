namespace ViridiscaUi.Infrastructure.ApiClient;

// ---- Academic Periods ----

public sealed record CreateAcademicPeriodRequestDto(string Name, string Code, DateTime StartDate, DateTime EndDate, int AcademicYear, string Type, string Description = "");

public sealed record UpdateAcademicPeriodRequestDto(string Name, string Description, DateTime StartDate, DateTime EndDate);

public sealed record AcademicPeriodResponseDto(
    Guid Uid, string Name, string Code, string Description, string Type, string Status,
    DateTime StartDate, DateTime EndDate, int AcademicYear, bool IsCurrent, DateTime CreatedAtUtc);

// ---- Course Instances ----

public sealed record CreateCourseInstanceRequestDto(
    Guid SubjectUid, Guid GroupUid, Guid AcademicPeriodUid, string Name, string Code,
    DateTime StartDate, Guid? TeacherUid = null, string Description = "", int MaxEnrollments = 30);

public sealed record UpdateCourseInstanceRequestDto(string Name, string Description, int MaxEnrollments);

public sealed record AssignTeacherRequestDto(Guid? TeacherUid);

public sealed record CourseInstanceResponseDto(
    Guid Uid, Guid SubjectUid, Guid GroupUid, Guid AcademicPeriodUid, Guid? TeacherUid,
    string Name, string Code, string Description, DateTime StartDate, DateTime? EndDate,
    int MaxEnrollments, string Status, DateTime CreatedAtUtc);

// ---- Assignments ----

public sealed record CreateAssignmentRequestDto(
    Guid CourseInstanceUid, string Title, string Description, string Type,
    decimal MaxScore = 100, DateTime? DueDate = null, string Instructions = "", string Difficulty = "Medium");

public sealed record UpdateAssignmentRequestDto(string Title, string Description, string Instructions, DateTime? DueDate, decimal MaxScore);

public sealed record AssignmentResponseDto(
    Guid Uid, Guid CourseInstanceUid, string Title, string Description, string Instructions,
    DateTime? DueDate, decimal MaxScore, string Type, string Difficulty, string Status,
    bool IsPublished, DateTime CreatedAtUtc);

// ---- Submissions ----

public sealed record CreateSubmissionRequestDto(Guid AssignmentUid, Guid StudentUid, string? Content = null, string? FilePath = null);

public sealed record GradeSubmissionRequestDto(decimal Score, string Feedback, Guid GradedByUid);

public sealed record SubmissionResponseDto(
    Guid Uid, Guid AssignmentUid, Guid StudentUid, string? Content, string? FilePath,
    DateTime SubmissionDate, string Status, decimal? Score, string? Feedback,
    Guid? GradedByUid, DateTime? GradedAtUtc, DateTime CreatedAtUtc);
