using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Maps backend Scheduler API DTOs to the client's own <see cref="ScheduleSlot"/>. Both
/// <see cref="DayOfWeek"/> (BCL enum) and <see cref="ScheduleSlotType"/> match the backend
/// member-for-member, so plain Enum.Parse is safe (same approach as CurriculumMappers).
/// </summary>
internal static class SchedulerMappers
{
    public static ScheduleSlot ToScheduleSlot(ScheduleSlotResponseDto dto) => new()
    {
        Uid = dto.Uid,
        CourseInstanceUid = dto.CourseInstanceUid,
        DayOfWeek = Enum.Parse<DayOfWeek>(dto.DayOfWeek),
        StartTime = dto.StartTime,
        EndTime = dto.EndTime,
        Room = dto.Room,
        StartDate = dto.StartDate,
        EndDate = dto.EndDate,
        Type = Enum.Parse<ScheduleSlotType>(dto.Type),
        IsActive = dto.IsActive,
        Notes = dto.Notes,
        MaxStudents = dto.MaxStudents,
    };
}
