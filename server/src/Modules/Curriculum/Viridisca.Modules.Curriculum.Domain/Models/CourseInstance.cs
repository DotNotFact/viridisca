using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

/// <summary>
/// Экземпляр курса: конкретный предмет, преподаваемый конкретной группе
/// конкретным преподавателем в конкретном учебном периоде
/// </summary>
public class CourseInstance : Entity
{
    public Guid Uid { get; private set; }
    public Guid SubjectUid { get; private set; }
    public Guid GroupUid { get; private set; }
    public Guid AcademicPeriodUid { get; private set; }
    public Guid? TeacherUid { get; private set; }
    public string Name { get; private set; }
    public string Code { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public int MaxEnrollments { get; private set; }
    public CourseInstanceStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    protected CourseInstance() { }

    public static Result<CourseInstance> Create(
        Guid subjectUid,
        Guid groupUid,
        Guid academicPeriodUid,
        string name,
        string code,
        DateTime startDate,
        Guid? teacherUid = null,
        string description = "",
        int maxEnrollments = 30)
    {
        if (subjectUid == Guid.Empty)
            return Result.Failure<CourseInstance>(new Error("SubjectUid.Empty", "ID предмета не может быть пустым", ErrorType.Validation));

        if (groupUid == Guid.Empty)
            return Result.Failure<CourseInstance>(new Error("GroupUid.Empty", "ID группы не может быть пустым", ErrorType.Validation));

        if (academicPeriodUid == Guid.Empty)
            return Result.Failure<CourseInstance>(new Error("AcademicPeriodUid.Empty", "ID учебного периода не может быть пустым", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<CourseInstance>(new Error("Name.Empty", "Название не может быть пустым", ErrorType.Validation));

        if (maxEnrollments <= 0)
            return Result.Failure<CourseInstance>(new Error("MaxEnrollments.Invalid", "Максимальное число студентов должно быть положительным", ErrorType.Validation));

        var courseInstance = new CourseInstance
        {
            Uid = Guid.NewGuid(),
            SubjectUid = subjectUid,
            GroupUid = groupUid,
            AcademicPeriodUid = academicPeriodUid,
            TeacherUid = teacherUid,
            Name = name.Trim(),
            Code = code?.Trim() ?? string.Empty,
            Description = description,
            StartDate = startDate,
            MaxEnrollments = maxEnrollments,
            Status = CourseInstanceStatus.Draft,
            CreatedAtUtc = DateTime.UtcNow
        };

        courseInstance.Raise(new CourseInstanceCreatedDomainEvent(courseInstance.Uid));
        return courseInstance;
    }

    public Result UpdateDetails(string name, string description, int maxEnrollments)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(new Error("Name.Empty", "Название не может быть пустым", ErrorType.Validation));

        if (maxEnrollments <= 0)
            return Result.Failure(new Error("MaxEnrollments.Invalid", "Максимальное число студентов должно быть положительным", ErrorType.Validation));

        Name = name.Trim();
        Description = description;
        MaxEnrollments = maxEnrollments;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void AssignTeacher(Guid? teacherUid)
    {
        TeacherUid = teacherUid;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void SetStatus(CourseInstanceStatus status)
    {
        Status = status;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public Result SetEndDate(DateTime endDate)
    {
        if (endDate <= StartDate)
            return Result.Failure(new Error("EndDate.Invalid", "Дата окончания должна быть позже даты начала", ErrorType.Validation));

        EndDate = endDate;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }
}

public enum CourseInstanceStatus
{
    Draft,
    Active,
    Completed,
    Cancelled
}
