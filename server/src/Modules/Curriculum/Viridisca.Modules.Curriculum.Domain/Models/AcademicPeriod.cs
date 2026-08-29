using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

/// <summary>
/// Учебный период (семестр, четверть, триместр)
/// </summary>
public class AcademicPeriod : Entity
{
    public Guid Uid { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public AcademicPeriodType Type { get; private set; }
    public AcademicPeriodStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int AcademicYear { get; private set; }
    public bool IsCurrent { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    protected AcademicPeriod() { }

    public static Result<AcademicPeriod> Create(
        string name,
        string code,
        DateTime startDate,
        DateTime endDate,
        int academicYear,
        AcademicPeriodType type,
        string description = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<AcademicPeriod>(new Error("Name.Empty", "Название периода не может быть пустым", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure<AcademicPeriod>(new Error("Code.Empty", "Код периода не может быть пустым", ErrorType.Validation));

        if (endDate <= startDate)
            return Result.Failure<AcademicPeriod>(new Error("Dates.Invalid", "Дата окончания должна быть позже даты начала", ErrorType.Validation));

        var period = new AcademicPeriod
        {
            Uid = Guid.NewGuid(),
            Name = name.Trim(),
            Code = code.Trim(),
            Description = description,
            Type = type,
            Status = AcademicPeriodStatus.Planned,
            StartDate = startDate,
            EndDate = endDate,
            AcademicYear = academicYear,
            IsCurrent = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        period.Raise(new AcademicPeriodCreatedDomainEvent(period.Uid));
        return period;
    }

    public Result UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Name.Empty", "Название периода не может быть пустым", ErrorType.Validation));

        Name = name.Trim();
        Description = description;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public Result UpdateDates(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
            return Result.Failure(new Error("Dates.Invalid", "Дата окончания должна быть позже даты начала", ErrorType.Validation));

        StartDate = startDate;
        EndDate = endDate;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void SetStatus(AcademicPeriodStatus status)
    {
        Status = status;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void SetAsCurrent(bool isCurrent)
    {
        IsCurrent = isCurrent;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}

public enum AcademicPeriodType
{
    Semester,
    Quarter,
    Trimester,
    Module,
    SummerSession,
    WinterSession
}

public enum AcademicPeriodStatus
{
    Planned,
    Active,
    Completed,
    Cancelled,
    Suspended
}
