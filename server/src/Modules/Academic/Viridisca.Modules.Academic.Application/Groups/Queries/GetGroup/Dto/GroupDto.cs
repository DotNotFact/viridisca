using System;

namespace Viridisca.Modules.Academic.Application.Groups.Queries.GetGroup.Dto;

public class GroupDto
{
    public Guid Uid { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Year { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int MaxStudents { get; set; }
    public int CurrentStudentsCount { get; set; }
    public string Status { get; set; }
    public Guid? CuratorUid { get; set; }
    public Guid DepartmentUid { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
