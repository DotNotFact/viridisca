namespace ViridiscaUi.Domain.Models;

/// <summary>
/// Общая статистика системы для главной панели
/// </summary>
/// <remarks>
/// Was previously an anonymous type returned as `object` from
/// IStatisticsService.GetSystemStatisticsAsync() and accessed via `dynamic` in
/// MainViewModel - anonymous types are compiler-generated as internal to their
/// declaring assembly, so accessing their members via `dynamic` from a different
/// assembly (ViridiscaUi.Infrastructure -> ViridiscaUi) throws a RuntimeBinderException
/// ("'object' does not contain a definition for 'TotalStudents'") even though the
/// property genuinely exists on the object. A real, public DTO avoids the whole
/// class of cross-assembly dynamic-binding bugs.
/// </remarks>
public sealed class SystemStatistics
{
    public int TotalStudents { get; set; }
    public int TotalTeachers { get; set; }
    public int TotalCourses { get; set; }
    public int TotalGroups { get; set; }
    public int TotalAssignments { get; set; }
    public DateTime LastUpdated { get; set; }
}
