using System;
using Viridisca.Common.Domain;

namespace Viridisca.Modules.Curriculum.Domain.Models;

/// <summary>
/// Работа студента, сданная по заданию
/// </summary>
public class Submission : Entity
{
    public Guid Uid { get; private set; }
    public Guid AssignmentUid { get; private set; }
    public Guid StudentUid { get; private set; }
    public string Content { get; private set; }
    public string FilePath { get; private set; }
    public DateTime SubmissionDate { get; private set; }
    public SubmissionStatus Status { get; private set; }
    public decimal? Score { get; private set; }
    public string Feedback { get; private set; }
    public Guid? GradedByUid { get; private set; }
    public DateTime? GradedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? LastModifiedAtUtc { get; private set; }

    protected Submission() { }

    public static Result<Submission> Create(
        Guid assignmentUid,
        Guid studentUid,
        string content = null,
        string filePath = null)
    {
        if (assignmentUid == Guid.Empty)
            return Result.Failure<Submission>(new Error("AssignmentUid.Empty", "ID задания не может быть пустым", ErrorType.Validation));

        if (studentUid == Guid.Empty)
            return Result.Failure<Submission>(new Error("StudentUid.Empty", "ID студента не может быть пустым", ErrorType.Validation));

        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(filePath))
            return Result.Failure<Submission>(new Error("Content.Empty", "Работа должна содержать текст или файл", ErrorType.Validation));

        var submission = new Submission
        {
            Uid = Guid.NewGuid(),
            AssignmentUid = assignmentUid,
            StudentUid = studentUid,
            Content = content,
            FilePath = filePath,
            SubmissionDate = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted,
            CreatedAtUtc = DateTime.UtcNow
        };

        submission.Raise(new SubmissionCreatedDomainEvent(submission.Uid));
        return submission;
    }

    public Result Grade(decimal score, string feedback, Guid gradedByUid)
    {
        if (score < 0)
            return Result.Failure(new Error("Score.Invalid", "Балл не может быть отрицательным", ErrorType.Validation));

        if (gradedByUid == Guid.Empty)
            return Result.Failure(new Error("GradedByUid.Empty", "ID проверяющего не может быть пустым", ErrorType.Validation));

        Score = score;
        Feedback = feedback;
        GradedByUid = gradedByUid;
        GradedAtUtc = DateTime.UtcNow;
        Status = SubmissionStatus.Graded;
        LastModifiedAtUtc = DateTime.UtcNow;

        Raise(new SubmissionGradedDomainEvent(Uid, score));

        return Result.Success();
    }

    public Result UpdateContent(string content, string filePath)
    {
        if (string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(filePath))
            return Result.Failure(new Error("Content.Empty", "Работа должна содержать текст или файл", ErrorType.Validation));

        Content = content;
        FilePath = filePath;
        LastModifiedAtUtc = DateTime.UtcNow;

        return Result.Success();
    }

    public void MarkAsLate()
    {
        Status = SubmissionStatus.Late;
        LastModifiedAtUtc = DateTime.UtcNow;
    }
}

public enum SubmissionStatus
{
    Submitted,
    Late,
    UnderReview,
    Graded,
    Returned
}
