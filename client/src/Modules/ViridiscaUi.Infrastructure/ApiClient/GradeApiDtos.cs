namespace ViridiscaUi.Infrastructure.ApiClient;

public sealed record CreateGradeRequestDto(Guid StudentUid, Guid SubjectUid, Guid TeacherUid, decimal Value, string Type, string? Description = null, Guid? LessonUid = null);

public sealed record UpdateGradeRequestDto(decimal Value, string? Description = null, string? Reason = null);

public sealed record GradeResponseDto(
    Guid Uid, Guid StudentUid, Guid SubjectUid, Guid TeacherUid, Guid? LessonUid,
    decimal Value, string Description, string Type, DateTime IssuedAtUtc,
    bool IsPublished, DateTime? PublishedAtUtc, DateTime CreatedAtUtc, DateTime? LastModifiedAtUtc);
