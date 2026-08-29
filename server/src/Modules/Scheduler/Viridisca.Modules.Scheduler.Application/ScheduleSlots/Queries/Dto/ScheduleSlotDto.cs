using System;

namespace Viridisca.Modules.Scheduler.Application.ScheduleSlots.Queries.Dto;

public sealed class ScheduleSlotDto
{
    public Guid Uid { get; set; }
    public Guid CourseInstanceUid { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string? Room { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int? MaxStudents { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
