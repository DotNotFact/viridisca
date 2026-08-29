using System;

namespace Viridisca.Modules.Curriculum.Application.Submissions.Queries.Dto;

public sealed class SubmissionDto
{
    public Guid Uid { get; set; }
    public Guid AssignmentUid { get; set; }
    public Guid StudentUid { get; set; }
    public string? Content { get; set; }
    public string? FilePath { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string? Feedback { get; set; }
    public Guid? GradedByUid { get; set; }
    public DateTime? GradedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
