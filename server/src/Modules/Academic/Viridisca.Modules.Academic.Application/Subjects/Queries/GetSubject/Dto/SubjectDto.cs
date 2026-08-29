using System;

namespace Viridisca.Modules.Academic.Application.Subjects.Queries.GetSubject.Dto;

public class SubjectDto
{
    public Guid Uid { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public int Credits { get; set; }
    public string Type { get; set; }
    public string Difficulty { get; set; }
    public Guid? DepartmentUid { get; set; }
    public int? MinimumRequiredGrade { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
