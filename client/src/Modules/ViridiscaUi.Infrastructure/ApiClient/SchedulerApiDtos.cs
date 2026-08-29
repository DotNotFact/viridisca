namespace ViridiscaUi.Infrastructure.ApiClient;

public sealed record CreateScheduleSlotRequestDto(
    Guid CourseInstanceUid,
    string DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime StartDate,
    DateTime? EndDate,
    string? Room,
    string Type,
    string Notes,
    int? MaxStudents);

public sealed record UpdateScheduleSlotRequestDto(
    string DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    DateTime StartDate,
    DateTime? EndDate,
    string? Room,
    string Type,
    string Notes,
    int? MaxStudents);

public sealed record ScheduleSlotResponseDto(
    Guid Uid,
    Guid CourseInstanceUid,
    string DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string? Room,
    DateTime StartDate,
    DateTime? EndDate,
    string Type,
    bool IsActive,
    string Notes,
    int? MaxStudents,
    DateTime CreatedAtUtc);
