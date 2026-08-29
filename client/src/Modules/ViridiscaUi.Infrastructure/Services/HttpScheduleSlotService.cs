using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;
using ViridiscaUi.Services;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IScheduleSlotService for the proven UI footprint (ScheduleViewModel:
/// GetAll/Create/Update/Delete; HomeViewModel: GetUpcomingSlots) plus the rest of basic
/// CRUD/lookup the backend supports (GetByUid, GetByCourseInstance). The backend enforces
/// room-conflict validation server-side on Create/Update (a gap the frontend's own EF
/// service never closed — see PROGRESS.md Phase 5), but doesn't expose it as a standalone
/// query, nor does it support teacher-scoped conflicts, statistics, auto-generation,
/// export/import, or paging/date-range/academic-period filters — those, and
/// GetAttendanceCountAsync (a stub even in the EF service, always returns 0), fall
/// through to <paramref name="inner"/>, the EF-backed ScheduleSlotService.
/// Note: like the EF service it replaces, GetUpcomingSlotsAsync ignores personUid and
/// returns the next N active slots globally — matching existing (if imprecise) behavior.
/// </summary>
public class HttpScheduleSlotService(ScheduleSlotApiClient apiClient, ScheduleSlotService inner, ILogger<HttpScheduleSlotService> logger) : IScheduleSlotService
{
    private readonly ScheduleSlotApiClient _apiClient = apiClient;
    private readonly ScheduleSlotService _inner = inner;
    private readonly ILogger<HttpScheduleSlotService> _logger = logger;

    public async Task<IEnumerable<ScheduleSlot>> GetAllAsync()
    {
        var (success, data, error) = await _apiClient.GetAllAsync();
        if (!success || data is null)
        {
            _logger.LogWarning("GetAllAsync (schedule slots) failed: {Error}", error);
            return [];
        }

        return data.Select(SchedulerMappers.ToScheduleSlot);
    }

    public Task<ScheduleSlot?> GetByIdAsync(Guid uid) => GetByUidAsync(uid);

    public async Task<ScheduleSlot?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({ScheduleSlotUid}) failed: {Error}", uid, error);
            return null;
        }

        return SchedulerMappers.ToScheduleSlot(data);
    }

    public async Task<IEnumerable<ScheduleSlot>> GetByCourseInstanceAsync(Guid courseInstanceUid)
    {
        var (success, data, error) = await _apiClient.GetByCourseInstanceAsync(courseInstanceUid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByCourseInstanceAsync({CourseInstanceUid}) failed: {Error}", courseInstanceUid, error);
            return [];
        }

        return data.Select(SchedulerMappers.ToScheduleSlot);
    }

    public Task<IEnumerable<ScheduleSlot>> GetByAcademicPeriodAsync(Guid academicPeriodUid) => _inner.GetByAcademicPeriodAsync(academicPeriodUid);

    public Task<IEnumerable<ScheduleSlot>> GetByDateRangeAsync(DateTime startDate, DateTime endDate) => _inner.GetByDateRangeAsync(startDate, endDate);

    public Task<(IEnumerable<ScheduleSlot> scheduleSlots, int totalCount)> GetPagedAsync(
        int page, int pageSize, string? searchTerm = null, Guid? teacherUid = null, Guid? groupUid = null,
        Guid? subjectUid = null, DayOfWeek? dayOfWeek = null, DateTime? fromDate = null, DateTime? toDate = null,
        Guid? academicPeriodUid = null)
        => _inner.GetPagedAsync(page, pageSize, searchTerm, teacherUid, groupUid, subjectUid, dayOfWeek, fromDate, toDate, academicPeriodUid);

    public async Task<ScheduleSlot> CreateAsync(ScheduleSlot scheduleSlot)
    {
        var request = new CreateScheduleSlotRequestDto(
            scheduleSlot.CourseInstanceUid, scheduleSlot.DayOfWeek.ToString(), scheduleSlot.StartTime, scheduleSlot.EndTime,
            scheduleSlot.StartDate, scheduleSlot.EndDate, scheduleSlot.Room, scheduleSlot.Type.ToString(),
            scheduleSlot.Notes ?? string.Empty, scheduleSlot.MaxStudents);

        var (success, slotUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create schedule slot: {error}");
        }

        scheduleSlot.Uid = slotUid;
        return scheduleSlot;
    }

    public async Task<ScheduleSlot> UpdateAsync(ScheduleSlot scheduleSlot)
    {
        var request = new UpdateScheduleSlotRequestDto(
            scheduleSlot.DayOfWeek.ToString(), scheduleSlot.StartTime, scheduleSlot.EndTime,
            scheduleSlot.StartDate, scheduleSlot.EndDate, scheduleSlot.Room, scheduleSlot.Type.ToString(),
            scheduleSlot.Notes ?? string.Empty, scheduleSlot.MaxStudents);

        var (success, error) = await _apiClient.UpdateAsync(scheduleSlot.Uid, request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to update schedule slot: {error}");
        }

        return scheduleSlot;
    }

    public async Task<bool> DeleteAsync(Guid uid)
    {
        var (success, error) = await _apiClient.DeleteAsync(uid);
        if (!success)
        {
            _logger.LogWarning("DeleteAsync({ScheduleSlotUid}) failed: {Error}", uid, error);
        }

        return success;
    }

    public async Task<bool> ExistsAsync(Guid uid) => await GetByUidAsync(uid) is not null;

    public async Task<int> GetCountAsync() => (await GetAllAsync()).Count();

    public Task<IEnumerable<ScheduleSlot>> GetConflictingSlots(ScheduleSlot slot) => _inner.GetConflictingSlots(slot);

    public Task<IEnumerable<ScheduleSlot>> GetTeacherConflicts(Guid teacherUid, DateTime date, TimeSpan startTime, TimeSpan endTime)
        => _inner.GetTeacherConflicts(teacherUid, date, startTime, endTime);

    public Task<int> GetAttendanceCountAsync(Guid scheduleSlotUid) => _inner.GetAttendanceCountAsync(scheduleSlotUid);

    public Task<IEnumerable<object>> GetAllConflictsAsync() => _inner.GetAllConflictsAsync();

    public Task<object> GetScheduleStatisticsAsync(Guid? academicPeriodUid = null, Guid? teacherUid = null, Guid? groupUid = null)
        => _inner.GetScheduleStatisticsAsync(academicPeriodUid, teacherUid, groupUid);

    public Task<IEnumerable<ScheduleSlot>> GetConflictingSlots(
        DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, Guid? excludeSlotUid = null, Guid? academicPeriodUid = null)
        => _inner.GetConflictingSlots(dayOfWeek, startTime, endTime, excludeSlotUid, academicPeriodUid);

    public Task<IEnumerable<ScheduleSlot>> GetTeacherConflicts(
        Guid teacherUid, DayOfWeek dayOfWeek, TimeSpan startTime, TimeSpan endTime, Guid? excludeSlotUid = null)
        => _inner.GetTeacherConflicts(teacherUid, dayOfWeek, startTime, endTime, excludeSlotUid);

    public Task<bool> GenerateAutoScheduleAsync(Guid academicPeriodUid) => _inner.GenerateAutoScheduleAsync(academicPeriodUid);

    public async Task<IEnumerable<ScheduleSlot>> GetUpcomingSlotsAsync(Guid personUid, int count = 10)
    {
        var (success, data, error) = await _apiClient.GetUpcomingAsync(count);
        if (!success || data is null)
        {
            _logger.LogWarning("GetUpcomingSlotsAsync({PersonUid}, {Count}) failed: {Error}", personUid, count, error);
            return [];
        }

        return data.Select(SchedulerMappers.ToScheduleSlot);
    }

    public Task<string> ExportScheduleAsync(DateTime startDate, DateTime endDate) => _inner.ExportScheduleAsync(startDate, endDate);

    public Task<ImportResult<T>> ImportScheduleAsync<T>(string filePath) => _inner.ImportScheduleAsync<T>(filePath);
}
