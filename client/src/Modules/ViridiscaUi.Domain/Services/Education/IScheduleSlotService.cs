using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Сервис для управления слотами расписания
/// </summary>
public interface IScheduleSlotService
{
    Task<IEnumerable<ScheduleSlot>> GetAllAsync();
    Task<ScheduleSlot?> GetByIdAsync(Guid uid);
    Task<ScheduleSlot?> GetByUidAsync(Guid uid);
    Task<IEnumerable<ScheduleSlot>> GetByCourseInstanceAsync(Guid courseInstanceUid);
    Task<IEnumerable<ScheduleSlot>> GetByAcademicPeriodAsync(Guid academicPeriodUid);
    Task<IEnumerable<ScheduleSlot>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<(IEnumerable<ScheduleSlot> scheduleSlots, int totalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? teacherUid = null,
        Guid? groupUid = null,
        Guid? subjectUid = null,
        DayOfWeek? dayOfWeek = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? academicPeriodUid = null);
    Task<ScheduleSlot> CreateAsync(ScheduleSlot scheduleSlot);
    Task<ScheduleSlot> UpdateAsync(ScheduleSlot scheduleSlot);
    Task<bool> DeleteAsync(Guid uid);
    Task<bool> ExistsAsync(Guid uid);
    Task<int> GetCountAsync();
    Task<IEnumerable<ScheduleSlot>> GetConflictingSlots(ScheduleSlot slot);
    Task<IEnumerable<ScheduleSlot>> GetTeacherConflicts(Guid teacherUid, DateTime date, TimeSpan startTime, TimeSpan endTime);
    Task<int> GetAttendanceCountAsync(Guid scheduleSlotUid);
    Task<IEnumerable<object>> GetAllConflictsAsync();
    Task<object> GetScheduleStatisticsAsync(Guid? academicPeriodUid = null, Guid? teacherUid = null, Guid? groupUid = null);
    Task<IEnumerable<ScheduleSlot>> GetConflictingSlots(
        DayOfWeek dayOfWeek, 
        TimeSpan startTime, 
        TimeSpan endTime, 
        Guid? excludeSlotUid = null,
        Guid? academicPeriodUid = null);
    Task<IEnumerable<ScheduleSlot>> GetTeacherConflicts(
        Guid teacherUid, 
        DayOfWeek dayOfWeek, 
        TimeSpan startTime, 
        TimeSpan endTime,
        Guid? excludeSlotUid = null);
    Task<bool> GenerateAutoScheduleAsync(Guid academicPeriodUid);
    /// <summary>
    /// Получает предстоящие слоты расписания для пользователя
    /// </summary>
    Task<IEnumerable<ScheduleSlot>> GetUpcomingSlotsAsync(Guid personUid, int count = 10);
    /// <summary>
    /// Экспортирует расписание
    /// </summary>
    Task<string> ExportScheduleAsync(DateTime startDate, DateTime endDate);
    /// <summary>
    /// Импортирует расписание из файла
    /// </summary>
    Task<ImportResult<T>> ImportScheduleAsync<T>(string filePath);
} 