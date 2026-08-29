using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Scheduler.Domain.Models;

/// <summary>
/// Слот еженедельного расписания для экземпляра курса
/// </summary>
public class ScheduleSlot : Entity
{
    public Guid Uid { get; private set; }
    public Guid CourseInstanceUid { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public string? Room { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public ScheduleSlotType Type { get; private set; }
    public bool IsActive { get; private set; }
    public string Notes { get; private set; }
    public int? MaxStudents { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    protected ScheduleSlot() { }

    public static Result<ScheduleSlot> Create(
        Guid courseInstanceUid,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        DateTime startDate,
        DateTime? endDate = null,
        string? room = null,
        ScheduleSlotType type = ScheduleSlotType.Lecture,
        string notes = "",
        int? maxStudents = null)
    {
        if (courseInstanceUid == Guid.Empty)
            return Result.Failure<ScheduleSlot>(new Error("CourseInstanceUid.Empty", "ID экземпляра курса не может быть пустым", ErrorType.Validation));

        if (endTime <= startTime)
            return Result.Failure<ScheduleSlot>(new Error("Time.Invalid", "Время окончания должно быть позже времени начала", ErrorType.Validation));

        if (endDate.HasValue && endDate.Value <= startDate)
            return Result.Failure<ScheduleSlot>(new Error("Dates.Invalid", "Дата окончания действия слота должна быть позже даты начала", ErrorType.Validation));

        var slot = new ScheduleSlot
        {
            Uid = Guid.NewGuid(),
            CourseInstanceUid = courseInstanceUid,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            StartDate = startDate,
            EndDate = endDate,
            Room = room,
            Type = type,
            Notes = notes,
            MaxStudents = maxStudents,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        slot.Raise(new ScheduleSlotCreatedDomainEvent(slot.Uid));
        return slot;
    }

    public Result UpdateSchedule(DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, DateTime startDate, DateTime? endDate)
    {
        if (endTime <= startTime)
            return Result.Failure(new Error("Time.Invalid", "Время окончания должно быть позже времени начала", ErrorType.Validation));

        if (endDate.HasValue && endDate.Value <= startDate)
            return Result.Failure(new Error("Dates.Invalid", "Дата окончания действия слота должна быть позже даты начала", ErrorType.Validation));

        DayOfWeek = dayOfWeek;
        StartTime = startTime;
        EndTime = endTime;
        StartDate = startDate;
        EndDate = endDate;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void UpdateDetails(string? room, ScheduleSlotType type, string notes, int? maxStudents)
    {
        Room = room;
        Type = type;
        Notes = notes;
        MaxStudents = maxStudents;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}

public enum ScheduleSlotType
{
    Lecture,
    Seminar,
    Laboratory,
    Practice,
    Consultation,
    Exam,
    Test,
    SelfStudy
}
