using System;

namespace Viridisca.Modules.Curriculum.Application.AcademicPeriods.Queries.Dto;

public sealed class AcademicPeriodDto
{
    public Guid Uid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int AcademicYear { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
