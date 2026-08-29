using System;

namespace Viridisca.Modules.Curriculum.Application.Assignments.Queries.Dto;

public sealed class AssignmentDto
{
    public Guid Uid { get; set; }
    public Guid CourseInstanceUid { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public decimal MaxScore { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
