using Microsoft.EntityFrameworkCore;
using Viridisca.Modules.Scheduler.Domain.Models;
using Viridisca.Modules.Scheduler.Domain.Repositories;
using Viridisca.Modules.Scheduler.Infrastructure.Database;

namespace Viridisca.Modules.Scheduler.Infrastructure.Repositories;

public class ScheduleSlotRepository(SchedulerDbContext dbContext) : IScheduleSlotRepository
{
    private readonly SchedulerDbContext _dbContext = dbContext;

    public async Task<ScheduleSlot> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleSlots.FirstOrDefaultAsync(s => s.Uid == uid, cancellationToken);

    public async Task<IReadOnlyList<ScheduleSlot>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleSlots
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ScheduleSlot>> GetByCourseInstanceAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleSlots
            .Where(s => s.CourseInstanceUid == courseInstanceUid)
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ScheduleSlot>> GetUpcomingAsync(int count, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;
        var currentTime = DateTime.UtcNow.TimeOfDay;
        var currentDayOfWeek = today.DayOfWeek;

        return await _dbContext.ScheduleSlots
            .Where(s => s.IsActive &&
                        s.StartDate <= today &&
                        (s.EndDate == null || s.EndDate >= today) &&
                        (s.DayOfWeek > currentDayOfWeek ||
                         (s.DayOfWeek == currentDayOfWeek && s.StartTime > currentTime)))
            .OrderBy(s => s.DayOfWeek)
            .ThenBy(s => s.StartTime)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScheduleSlot>> GetRoomConflictsAsync(
        string room,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        DateTime startDate,
        DateTime? endDate,
        Guid? excludeSlotUid = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ScheduleSlots
            .Where(s => s.IsActive &&
                        s.Room == room &&
                        s.DayOfWeek == dayOfWeek &&
                        s.StartTime < endTime && s.EndTime > startTime &&
                        s.StartDate <= (endDate ?? DateTime.MaxValue) &&
                        (s.EndDate == null || s.EndDate >= startDate) &&
                        (excludeSlotUid == null || s.Uid != excludeSlotUid))
            .ToListAsync(cancellationToken);
    }

    public void Insert(ScheduleSlot scheduleSlot) => _dbContext.ScheduleSlots.Add(scheduleSlot);

    public void Update(ScheduleSlot scheduleSlot) => _dbContext.ScheduleSlots.Update(scheduleSlot);

    public void Delete(ScheduleSlot scheduleSlot) => _dbContext.ScheduleSlots.Remove(scheduleSlot);
}
