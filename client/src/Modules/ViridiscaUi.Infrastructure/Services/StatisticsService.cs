using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education.Enums;
using Microsoft.EntityFrameworkCore;
using ViridiscaUi.Infrastructure.Data;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Entities.Analytics;
using ViridiscaUi.Domain.Models;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// Сервис для работы с аналитикой и статистикой
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly ApplicationDbContext _context;

    public StatisticsService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Получает аналитику студента
    /// </summary>
    public async Task<StudentAnalytics> GetStudentAnalyticsAsync(Guid studentUid, Guid? academicPeriodUid = null)
    {
        try
        {
            // Используем текущий академический период, если не указан
            if (!academicPeriodUid.HasValue)
            {
                var currentPeriod = await _context.AcademicPeriods
                    .Where(ap => ap.StartDate <= DateTime.UtcNow && ap.EndDate >= DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                academicPeriodUid = currentPeriod?.Uid ?? Guid.Empty;
            }

            // Ищем существующую аналитику
            var analytics = await _context.StudentAnalytics
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.Grades)
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.Enrollments)
                        .ThenInclude(e => e.CourseInstance)
                            .ThenInclude(ci => ci!.Subject)
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.Submissions)
                .Include(sa => sa.Student)
                    .ThenInclude(s => s!.QuizAttempts)
                .Include(sa => sa.AcademicPeriod)
                .FirstOrDefaultAsync(sa => sa.StudentUid == studentUid && sa.AcademicPeriodUid == academicPeriodUid);

            // Если не найдена, создаем новую
            if (analytics == null)
            {
                analytics = new StudentAnalytics
                {
                    StudentUid = studentUid,
                    AcademicPeriodUid = academicPeriodUid.Value,
                    LastCalculated = DateTime.UtcNow
                };

                _context.StudentAnalytics.Add(analytics);
                await _context.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _context.StudentAnalytics
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.Grades)
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.Enrollments)
                            .ThenInclude(e => e.CourseInstance)
                                .ThenInclude(ci => ci!.Subject)
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.Submissions)
                    .Include(sa => sa.Student)
                        .ThenInclude(s => s!.QuizAttempts)
                    .Include(sa => sa.AcademicPeriod)
                    .FirstAsync(sa => sa.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно
            if (analytics.ShouldRecalculate(TimeSpan.FromHours(1)))
            {
                await UpdateStudentAnalyticsCachedFields(analytics);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при получении аналитики студента {studentUid}");
            throw;
        }
    }

    /// <summary>
    /// Получает аналитику преподавателя
    /// </summary>
    public async Task<TeacherAnalytics> GetTeacherAnalyticsAsync(Guid teacherUid)
    {
        try
        {
            var analytics = await _context.TeacherAnalytics
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CourseInstances)
                        .ThenInclude(ci => ci.Enrollments)
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CourseInstances)
                        .ThenInclude(ci => ci.Assignments)
                            .ThenInclude(a => a.Submissions)
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CourseInstances)
                        .ThenInclude(ci => ci.Exams)
                .Include(ta => ta.Teacher)
                    .ThenInclude(t => t!.CuratorGroups)
                .FirstOrDefaultAsync(ta => ta.TeacherUid == teacherUid);

            if (analytics == null)
            {
                analytics = new TeacherAnalytics
                {
                    TeacherUid = teacherUid,
                    LastCalculated = DateTime.UtcNow
                };

                _context.TeacherAnalytics.Add(analytics);
                await _context.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _context.TeacherAnalytics
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CourseInstances)
                            .ThenInclude(ci => ci.Enrollments)
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CourseInstances)
                            .ThenInclude(ci => ci.Assignments)
                                .ThenInclude(a => a.Submissions)
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CourseInstances)
                            .ThenInclude(ci => ci.Exams)
                    .Include(ta => ta.Teacher)
                        .ThenInclude(t => t!.CuratorGroups)
                    .FirstAsync(ta => ta.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно
            if (analytics.ShouldRecalculate(TimeSpan.FromHours(1)))
            {
                await UpdateTeacherAnalyticsCachedFields(analytics);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при получении аналитики преподавателя {teacherUid}");
            throw;
        }
    }

    /// <summary>
    /// Получает аналитику курса
    /// </summary>
    public async Task<CourseAnalytics> GetCourseAnalyticsAsync(Guid courseInstanceUid)
    {
        try
        {
            var analytics = await _context.CourseAnalytics
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Enrollments)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Assignments)
                        .ThenInclude(a => a.Submissions)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Quizzes)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Discussions)
                        .ThenInclude(d => d.Posts)
                .Include(ca => ca.CourseInstance)
                    .ThenInclude(ci => ci!.Exams)
                .FirstOrDefaultAsync(ca => ca.CourseInstanceUid == courseInstanceUid);

            if (analytics == null)
            {
                analytics = new CourseAnalytics
                {
                    CourseInstanceUid = courseInstanceUid,
                    LastCalculated = DateTime.UtcNow
                };

                _context.CourseAnalytics.Add(analytics);
                await _context.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _context.CourseAnalytics
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Enrollments)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Assignments)
                            .ThenInclude(a => a.Submissions)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Quizzes)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Discussions)
                            .ThenInclude(d => d.Posts)
                    .Include(ca => ca.CourseInstance)
                        .ThenInclude(ci => ci!.Exams)
                    .FirstAsync(ca => ca.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно
            if (analytics.ShouldRecalculate(TimeSpan.FromHours(1)))
            {
                await UpdateCourseAnalyticsCachedFields(analytics);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при получении аналитики курса {courseInstanceUid}");
            throw;
        }
    }

    /// <summary>
    /// Получает аналитику группы
    /// </summary>
    public async Task<GroupAnalytics> GetGroupAnalyticsAsync(Guid groupUid)
    {
        try
        {
            var analytics = await _context.GroupAnalytics
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Students)
                        .ThenInclude(s => s.Grades)
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Students)
                        .ThenInclude(s => s.Enrollments)
                            .ThenInclude(e => e.CourseInstance)
                                .ThenInclude(ci => ci!.Assignments)
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Students)
                        .ThenInclude(s => s.Submissions)
                .Include(ga => ga.Group)
                    .ThenInclude(g => g!.Curator)
                .FirstOrDefaultAsync(ga => ga.GroupUid == groupUid);

            if (analytics == null)
            {
                analytics = new GroupAnalytics
                {
                    GroupUid = groupUid,
                    LastCalculated = DateTime.UtcNow
                };

                _context.GroupAnalytics.Add(analytics);
                await _context.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _context.GroupAnalytics
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Students)
                            .ThenInclude(s => s.Grades)
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Students)
                            .ThenInclude(s => s.Enrollments)
                                .ThenInclude(e => e.CourseInstance)
                                    .ThenInclude(ci => ci!.Assignments)
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Students)
                            .ThenInclude(s => s.Submissions)
                    .Include(ga => ga.Group)
                        .ThenInclude(g => g!.Curator)
                    .FirstAsync(ga => ga.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно
            if (analytics.ShouldRecalculate(TimeSpan.FromHours(1)))
            {
                await UpdateGroupAnalyticsCachedFields(analytics);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при получении аналитики группы {groupUid}");
            throw;
        }
    }

    /// <summary>
    /// Получает аналитику задания
    /// </summary>
    public async Task<AssignmentAnalytics> GetAssignmentAnalyticsAsync(Guid assignmentUid)
    {
        try
        {
            var analytics = await _context.AssignmentAnalytics
                .Include(aa => aa.Assignment)
                    .ThenInclude(a => a!.CourseInstance)
                        .ThenInclude(ci => ci!.Enrollments)
                .Include(aa => aa.Assignment)
                    .ThenInclude(a => a!.Submissions)
                .FirstOrDefaultAsync(aa => aa.AssignmentUid == assignmentUid);

            if (analytics == null)
            {
                analytics = new AssignmentAnalytics
                {
                    AssignmentUid = assignmentUid,
                    LastCalculated = DateTime.UtcNow
                };

                _context.AssignmentAnalytics.Add(analytics);
                await _context.SaveChangesAsync();

                // Перезагружаем с навигационными свойствами
                analytics = await _context.AssignmentAnalytics
                    .Include(aa => aa.Assignment)
                        .ThenInclude(a => a!.CourseInstance)
                            .ThenInclude(ci => ci!.Enrollments)
                    .Include(aa => aa.Assignment)
                        .ThenInclude(a => a!.Submissions)
                    .FirstAsync(aa => aa.Uid == analytics.Uid);
            }

            // Обновляем cached поля если нужно
            if (analytics.ShouldRecalculate(TimeSpan.FromHours(1)))
            {
                await UpdateAssignmentAnalyticsCachedFields(analytics);
            }

            return analytics;
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Ошибка при получении аналитики задания {assignmentUid}");
            throw;
        }
    }

    public async Task RefreshAllAnalyticsAsync()
    {
        var studentAnalyticsTask = _context.StudentAnalytics.ToListAsync();
        var teacherAnalyticsTask = _context.TeacherAnalytics.ToListAsync();
        var courseAnalyticsTask = _context.CourseAnalytics.ToListAsync();
        var groupAnalyticsTask = _context.GroupAnalytics.ToListAsync();
        var assignmentAnalyticsTask = _context.AssignmentAnalytics.ToListAsync();

        await Task.WhenAll([studentAnalyticsTask, teacherAnalyticsTask, courseAnalyticsTask, groupAnalyticsTask, assignmentAnalyticsTask,]);

        var studentAnalytics = await studentAnalyticsTask;
        var teacherAnalytics = await teacherAnalyticsTask;
        var courseAnalytics = await courseAnalyticsTask;
        var groupAnalytics = await groupAnalyticsTask;
        var assignmentAnalytics = await assignmentAnalyticsTask;

        foreach (var analytics in studentAnalytics)
        {
            analytics.RefreshCalculatedFields();
        }

        foreach (var analytics in teacherAnalytics)
        {
            analytics.RefreshCalculatedFields();
        }

        foreach (var analytics in courseAnalytics)
        {
            analytics.RefreshCalculatedFields();
        }

        foreach (var analytics in groupAnalytics)
        {
            analytics.RefreshCalculatedFields();
        }

        foreach (var analytics in assignmentAnalytics)
        {
            analytics.RefreshCalculatedFields();
        }

        await _context.SaveChangesAsync();
    }

    #region Private Helper Methods

    private async Task UpdateStudentAnalyticsCachedFields(StudentAnalytics analytics)
    {
        // Обновляем процент посещаемости через запрос к БД
        var attendanceQuery = await _context.Attendances
            .Where(a => a.StudentUid == analytics.StudentUid)
            .Where(a => a.Lesson!.CourseInstance!.AcademicPeriodUid == analytics.AcademicPeriodUid)
            .ToListAsync();

        if (attendanceQuery.Any())
        {
            var totalClasses = attendanceQuery.Count;
            var attendedClasses = attendanceQuery.Count(a => a.Status == ViridiscaUi.Domain.Entities.System.Enums.AttendanceStatus.Present);
            analytics.AttendancePercentage = totalClasses > 0 ? (decimal)attendedClasses / totalClasses * 100 : 0m;
        }

        analytics.RefreshCalculatedFields();
        await _context.SaveChangesAsync();
    }

    private async Task UpdateTeacherAnalyticsCachedFields(TeacherAnalytics analytics)
    {
        // Обновляем средние оценки через запрос к БД
        var grades = await _context.Grades
            .Where(g => g.TeacherUid == analytics.TeacherUid)
            .Where(g => g.Value > 0)
            .ToListAsync();

        if (grades.Any())
        {
            analytics.AverageGrade = grades.Average(g => g.Value);
        }

        // Обновляем посещаемость
        var attendances = await _context.Attendances
            .Where(a => a.Lesson!.CourseInstance!.TeacherUid == analytics.TeacherUid)
            .ToListAsync();

        if (attendances.Any())
        {
            var totalClasses = attendances.Count;
            var attendedClasses = attendances.Count(a => a.Status == ViridiscaUi.Domain.Entities.System.Enums.AttendanceStatus.Present);
            analytics.AverageAttendance = totalClasses > 0 ? (decimal)attendedClasses / totalClasses * 100 : 0m;
        }

        analytics.RefreshCalculatedFields();
        await _context.SaveChangesAsync();
    }

    private async Task UpdateCourseAnalyticsCachedFields(CourseAnalytics analytics)
    {
        // Обновляем средние оценки через запрос к БД
        var grades = await _context.Grades
            .Where(g => g.CourseInstanceUid == analytics.CourseInstanceUid)
            .Where(g => g.Value > 0)
            .ToListAsync();

        if (grades.Any())
        {
            analytics.AverageGrade = grades.Average(g => g.Value);
        }

        // Обновляем посещаемость
        var attendances = await _context.Attendances
            .Where(a => a.Lesson!.CourseInstanceUid == analytics.CourseInstanceUid)
            .ToListAsync();

        if (attendances.Any())
        {
            var totalClasses = attendances.Count;
            var attendedClasses = attendances.Count(a => a.Status == ViridiscaUi.Domain.Entities.System.Enums.AttendanceStatus.Present);
            analytics.AverageAttendance = totalClasses > 0 ? (decimal)attendedClasses / totalClasses * 100 : 0m;
        }

        analytics.RefreshCalculatedFields();
        await _context.SaveChangesAsync();
    }

    private async Task UpdateGroupAnalyticsCachedFields(GroupAnalytics analytics)
    {
        // Обновляем средний GPA группы
        var studentGPAs = await _context.Students
            .Where(s => s.GroupUid == analytics.GroupUid)
            .Where(s => s.Status == StudentStatus.Active)
            .Where(s => s.GPA > 0)
            .Select(s => s.GPA)
            .ToListAsync();

        if (studentGPAs.Any())
        {
            analytics.AverageGPA = (decimal)studentGPAs.Average();
        }

        // Обновляем посещаемость группы
        var attendances = await _context.Attendances
            .Where(a => a.Student!.GroupUid == analytics.GroupUid)
            .ToListAsync();

        if (attendances.Any())
        {
            var totalClasses = attendances.Count;
            var attendedClasses = attendances.Count(a => a.Status == ViridiscaUi.Domain.Entities.System.Enums.AttendanceStatus.Present);
            analytics.AverageAttendance = totalClasses > 0 ? (decimal)attendedClasses / totalClasses * 100 : 0m;
        }

        analytics.RefreshCalculatedFields();
        await _context.SaveChangesAsync();
    }

    private async Task UpdateAssignmentAnalyticsCachedFields(AssignmentAnalytics analytics)
    {
        // Обновляем средние оценки по заданию
        var grades = await _context.Grades
            .Where(g => g.AssignmentUid == analytics.AssignmentUid)
            .Where(g => g.Value > 0)
            .ToListAsync();

        if (grades.Any())
        {
            analytics.AverageGrade = grades.Average(g => g.Value);
            analytics.MaxGrade = grades.Max(g => g.Value);
            analytics.MinGrade = grades.Min(g => g.Value);
        }

        analytics.RefreshCalculatedFields();
        await _context.SaveChangesAsync();
    }

    #endregion

    /// <summary>
    /// Получает системную статистику
    /// </summary>
    public async Task<SystemStatistics> GetSystemStatisticsAsync()
    {
        try
        {
            var totalStudents = await _context.Students.CountAsync(s => !s.IsDeleted);
            var totalTeachers = await _context.Teachers.CountAsync(t => !t.IsDeleted);
            var totalCourses = await _context.CourseInstances.CountAsync(ci => ci.IsActive);
            var totalGroups = await _context.Groups.CountAsync(g => !g.IsDeleted);
            // Assignment.IsActive is a computed (get-only) property - EF can't translate it
            // to SQL. Inline the same condition as a real expression instead.
            var totalAssignments = await _context.Assignments.CountAsync(a =>
                a.Status != AssignmentStatus.Cancelled && a.Status != AssignmentStatus.Archived && !a.IsDeleted);

            return new SystemStatistics
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalCourses = totalCourses,
                TotalGroups = totalGroups,
                TotalAssignments = totalAssignments,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error getting system statistics: {ex.Message}", nameof(StatisticsService));
            return new SystemStatistics
            {
                TotalStudents = 0,
                TotalTeachers = 0,
                TotalCourses = 0,
                TotalGroups = 0,
                TotalAssignments = 0,
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}