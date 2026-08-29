using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

/// <summary>
/// Учебное задание в рамках экземпляра курса
/// </summary>
public class Assignment : Entity
{
    public Guid Uid { get; private set; }
    public Guid CourseInstanceUid { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Instructions { get; private set; }
    public DateTime? DueDate { get; private set; }
    public decimal MaxScore { get; private set; }
    public AssignmentType Type { get; private set; }
    public AssignmentDifficulty Difficulty { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public bool IsPublished { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    protected Assignment() { }

    public static Result<Assignment> Create(
        Guid courseInstanceUid,
        string title,
        string description,
        AssignmentType type,
        decimal maxScore = 100,
        DateTime? dueDate = null,
        string instructions = "",
        AssignmentDifficulty difficulty = AssignmentDifficulty.Medium)
    {
        if (courseInstanceUid == Guid.Empty)
            return Result.Failure<Assignment>(new Error("CourseInstanceUid.Empty", "ID экземпляра курса не может быть пустым", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<Assignment>(new Error("Title.Empty", "Название задания не может быть пустым", ErrorType.Validation));

        if (maxScore <= 0)
            return Result.Failure<Assignment>(new Error("MaxScore.Invalid", "Максимальный балл должен быть положительным", ErrorType.Validation));

        var assignment = new Assignment
        {
            Uid = Guid.NewGuid(),
            CourseInstanceUid = courseInstanceUid,
            Title = title.Trim(),
            Description = description,
            Instructions = instructions,
            DueDate = dueDate,
            MaxScore = maxScore,
            Type = type,
            Difficulty = difficulty,
            Status = AssignmentStatus.Draft,
            IsPublished = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        assignment.Raise(new AssignmentCreatedDomainEvent(assignment.Uid));
        return assignment;
    }

    public Result UpdateDetails(string title, string description, string instructions)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(new Error("Title.Empty", "Название задания не может быть пустым", ErrorType.Validation));

        Title = title.Trim();
        Description = description;
        Instructions = instructions;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void UpdateDueDate(DateTime? dueDate)
    {
        DueDate = dueDate;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public Result UpdateMaxScore(decimal maxScore)
    {
        if (maxScore <= 0)
            return Result.Failure(new Error("MaxScore.Invalid", "Максимальный балл должен быть положительным", ErrorType.Validation));

        MaxScore = maxScore;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void Publish()
    {
        IsPublished = true;
        Status = AssignmentStatus.Published;
        LastModifiedAtUtc = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = AssignmentStatus.Closed;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}

public enum AssignmentType
{
    Homework,
    Quiz,
    Exam,
    Project,
    LabWork,
    Test,
    Essay
}

public enum AssignmentDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum AssignmentStatus
{
    Draft,
    Published,
    Closed,
    Archived
}
