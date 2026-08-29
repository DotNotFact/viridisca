using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viridisca.Modules.Scheduler.Domain.Models;

namespace Viridisca.Modules.Scheduler.Domain.Repositories;

public interface IScheduleSlotRepository
{
    Task<ScheduleSlot> GetByUidAsync(Guid uid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduleSlot>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduleSlot>> GetByCourseInstanceAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ScheduleSlot>> GetUpcomingAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// Слоты, конфликтующие по аудитории (пересечение дня недели, времени и диапазона дат).
    /// </summary>
    Task<IReadOnlyList<ScheduleSlot>> GetRoomConflictsAsync(
        string room,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        DateTime startDate,
        DateTime? endDate,
        Guid? excludeSlotUid = null,
        CancellationToken cancellationToken = default);

    void Insert(ScheduleSlot scheduleSlot);
    void Update(ScheduleSlot scheduleSlot);
    void Delete(ScheduleSlot scheduleSlot);
}
