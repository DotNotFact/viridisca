using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Models;

namespace ViridiscaUi.Services
{
    /// <summary>
    /// Реализация сервиса для управления слотами расписания
    /// </summary>
    public class ScheduleSlotService : IScheduleSlotService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<ScheduleSlotService> _logger;

        public ScheduleSlotService(ApplicationDbContext dbContext, ILogger<ScheduleSlotService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<ScheduleSlot>> GetAllAsync()
        {
            try
            {
                return await _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Group)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Teacher)
                            .ThenInclude(t => t.Person)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all schedule slots");
                throw;
            }
        }

        public async Task<ScheduleSlot?> GetByIdAsync(Guid uid)
        {
            return await GetByUidAsync(uid);
        }

        public async Task<ScheduleSlot?> GetByUidAsync(Guid uid)
        {
            try
            {
                return await _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Group)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Teacher)
                            .ThenInclude(t => t.Person)
                    .FirstOrDefaultAsync(s => s.Uid == uid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule slot by UID: {Uid}", uid);
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetByCourseInstanceAsync(Guid courseInstanceUid)
        {
            try
            {
                return await _dbContext.ScheduleSlots
                    .Where(s => s.CourseInstanceUid == courseInstanceUid)
                    .Include(s => s.CourseInstance)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule slots by course instance: {CourseInstanceUid}", courseInstanceUid);
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetByAcademicPeriodAsync(Guid academicPeriodUid)
        {
            try
            {
                return await _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                    .Where(s => s.CourseInstance.AcademicPeriodUid == academicPeriodUid)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule slots by academic period: {AcademicPeriodUid}", academicPeriodUid);
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _dbContext.ScheduleSlots
                    .Where(s => s.ValidFrom <= endDate && (s.ValidTo == null || s.ValidTo >= startDate))
                    .Include(s => s.CourseInstance)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule slots by date range: {StartDate} - {EndDate}", startDate, endDate);
                throw;
            }
        }

        public async Task<(IEnumerable<ScheduleSlot> scheduleSlots, int totalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            string? searchTerm = null,
            Guid? teacherUid = null,
            Guid? groupUid = null,
            Guid? subjectUid = null,
            DayOfWeek? dayOfWeek = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            Guid? academicPeriodUid = null)
        {
            try
            {
                var query = _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Group)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Teacher)
                            .ThenInclude(t => t.Person)
                    .AsQueryable();

                // Применяем фильтры
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(s => 
                        s.Room.Contains(searchTerm) ||
                        s.CourseInstance.Subject.Name.Contains(searchTerm) ||
                        s.CourseInstance.Group.Name.Contains(searchTerm));
                }

                if (teacherUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.TeacherUid == teacherUid.Value);
                }

                if (groupUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.GroupUid == groupUid.Value);
                }

                if (subjectUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.SubjectUid == subjectUid.Value);
                }

                if (dayOfWeek.HasValue)
                {
                    query = query.Where(s => s.DayOfWeek == dayOfWeek.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(s => s.ValidFrom >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(s => s.ValidTo == null || s.ValidTo <= toDate.Value);
                }

                if (academicPeriodUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.AcademicPeriodUid == academicPeriodUid.Value);
                }

                var totalCount = await query.CountAsync();
                var scheduleSlots = await query
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (scheduleSlots, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paged schedule slots");
                throw;
            }
        }

        public async Task<ScheduleSlot> CreateAsync(ScheduleSlot scheduleSlot)
        {
            try
            {
                scheduleSlot.Uid = Guid.NewGuid();
                scheduleSlot.CreatedAt = DateTime.UtcNow;
                scheduleSlot.LastModifiedAt = DateTime.UtcNow;

                _dbContext.ScheduleSlots.Add(scheduleSlot);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Schedule slot created: {Uid}", scheduleSlot.Uid);
                return scheduleSlot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating schedule slot");
                throw;
            }
        }

        public async Task<ScheduleSlot> UpdateAsync(ScheduleSlot scheduleSlot)
        {
            try
            {
                scheduleSlot.LastModifiedAt = DateTime.UtcNow;
                _dbContext.ScheduleSlots.Update(scheduleSlot);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Schedule slot updated: {Uid}", scheduleSlot.Uid);
                return scheduleSlot;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule slot: {Uid}", scheduleSlot.Uid);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Guid uid)
        {
            try
            {
                var scheduleSlot = await _dbContext.ScheduleSlots.FindAsync(uid);
                if (scheduleSlot == null)
                    return false;

                _dbContext.ScheduleSlots.Remove(scheduleSlot);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Schedule slot deleted: {Uid}", uid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting schedule slot: {Uid}", uid);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid uid)
        {
            try
            {
                return await _dbContext.ScheduleSlots.AnyAsync(s => s.Uid == uid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if schedule slot exists: {Uid}", uid);
                throw;
            }
        }

        public async Task<int> GetCountAsync()
        {
            try
            {
                return await _dbContext.ScheduleSlots.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule slots count");
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetConflictingSlots(ScheduleSlot slot)
        {
            try
            {
                return await _dbContext.ScheduleSlots
                    .Where(s => s.Uid != slot.Uid &&
                               s.DayOfWeek == slot.DayOfWeek &&
                               s.StartTime < slot.EndTime &&
                               s.EndTime > slot.StartTime &&
                               s.ValidFrom <= slot.ValidTo &&
                               (s.ValidTo == null || s.ValidTo >= slot.ValidFrom))
                    .Include(s => s.CourseInstance)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conflicting slots");
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetTeacherConflicts(Guid teacherUid, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                var dayOfWeek = date.DayOfWeek;
                return await _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                    .Where(s => s.CourseInstance.TeacherUid == teacherUid &&
                               s.DayOfWeek == dayOfWeek &&
                               s.StartTime < endTime &&
                               s.EndTime > startTime &&
                               s.ValidFrom <= date &&
                               (s.ValidTo == null || s.ValidTo >= date))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher conflicts for teacher: {TeacherUid}", teacherUid);
                throw;
            }
        }

        public async Task<int> GetAttendanceCountAsync(Guid scheduleSlotUid)
        {
            try
            {
                // TODO: Реализовать подсчет посещаемости
                await Task.Delay(1);
                return 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attendance count for slot: {ScheduleSlotUid}", scheduleSlotUid);
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetAllConflictsAsync()
        {
            try
            {
                // TODO: Реализовать поиск всех конфликтов
                await Task.Delay(1);
                return new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all conflicts");
                throw;
            }
        }

        public async Task<object> GetScheduleStatisticsAsync(Guid? academicPeriodUid = null, Guid? teacherUid = null, Guid? groupUid = null)
        {
            try
            {
                var query = _dbContext.ScheduleSlots.AsQueryable();

                if (academicPeriodUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.AcademicPeriodUid == academicPeriodUid.Value);
                }

                if (teacherUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.TeacherUid == teacherUid.Value);
                }

                if (groupUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.GroupUid == groupUid.Value);
                }

                var totalSlots = await query.CountAsync();
                var uniqueDays = await query.Select(s => s.DayOfWeek).Distinct().CountAsync();

                return new
                {
                    TotalSlots = totalSlots,
                    UniqueDays = uniqueDays,
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schedule statistics");
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetConflictingSlots(
            DayOfWeek dayOfWeek, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            Guid? excludeSlotUid = null,
            Guid? academicPeriodUid = null)
        {
            try
            {
                var query = _dbContext.ScheduleSlots
                    .Where(s => s.DayOfWeek == dayOfWeek &&
                               s.StartTime < endTime &&
                               s.EndTime > startTime);

                if (excludeSlotUid.HasValue)
                {
                    query = query.Where(s => s.Uid != excludeSlotUid.Value);
                }

                if (academicPeriodUid.HasValue)
                {
                    query = query.Where(s => s.CourseInstance.AcademicPeriodUid == academicPeriodUid.Value);
                }

                return await query.Include(s => s.CourseInstance).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conflicting slots");
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetTeacherConflicts(
            Guid teacherUid, 
            DayOfWeek dayOfWeek, 
            TimeSpan startTime, 
            TimeSpan endTime,
            Guid? excludeSlotUid = null)
        {
            try
            {
                var query = _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                    .Where(s => s.CourseInstance.TeacherUid == teacherUid &&
                               s.DayOfWeek == dayOfWeek &&
                               s.StartTime < endTime &&
                               s.EndTime > startTime);

                if (excludeSlotUid.HasValue)
                {
                    query = query.Where(s => s.Uid != excludeSlotUid.Value);
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher conflicts");
                throw;
            }
        }

        public async Task<bool> GenerateAutoScheduleAsync(Guid academicPeriodUid)
        {
            try
            {
                // TODO: Реализовать автоматическое создание расписания
                await Task.Delay(1);
                _logger.LogInformation("Auto schedule generation requested for period: {AcademicPeriodUid}", academicPeriodUid);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating auto schedule for period: {AcademicPeriodUid}", academicPeriodUid);
                throw;
            }
        }

        public async Task<IEnumerable<ScheduleSlot>> GetUpcomingSlotsAsync(Guid personUid, int count = 10)
        {
            try
            {
                var today = DateTime.Today;
                var currentTime = DateTime.Now.TimeOfDay;
                var currentDayOfWeek = today.DayOfWeek;

                return await _dbContext.ScheduleSlots
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Subject)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Group)
                    .Include(s => s.CourseInstance)
                        .ThenInclude(ci => ci.Teacher)
                            .ThenInclude(t => t.Person)
                    .Where(s => s.ValidFrom <= today && 
                               (s.ValidTo == null || s.ValidTo >= today) &&
                               (s.DayOfWeek > currentDayOfWeek || 
                                s.DayOfWeek == currentDayOfWeek && s.StartTime > currentTime))
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.StartTime)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting upcoming slots for person: {PersonUid}", personUid);
                throw;
            }
        }

        public async Task<string> ExportScheduleAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // TODO: Реализовать экспорт расписания
                await Task.Delay(1);
                var fileName = $"schedule_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.xlsx";
                _logger.LogInformation("Schedule export requested: {FileName}", fileName);
                return fileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting schedule");
                throw;
            }
        }

        /// <summary>
        /// Импортирует расписание из файла
        /// </summary>
        public async Task<ImportResult<T>> ImportScheduleAsync<T>(string filePath)
        {
            try
            {
                // Заглушка для импорта расписания
                await Task.CompletedTask;
                
                return ImportResult<T>.Success(0, new List<T>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing schedule from {FilePath}", filePath);
                return ImportResult<T>.Failure(new List<string> { $"Ошибка импорта: {ex.Message}" });
            }
        }
    }
} 