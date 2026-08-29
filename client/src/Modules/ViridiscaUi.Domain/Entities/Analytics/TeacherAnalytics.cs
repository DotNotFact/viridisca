using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика преподавателя
/// </summary>
public class TeacherAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор преподавателя
    /// </summary>
    public Guid TeacherUid { get; set; }

    /// <summary>
    /// Идентификатор академического периода
    /// </summary>
    public Guid AcademicPeriodUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===
    
    /// <summary>
    /// Время последнего обновления статистики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Рейтинг преподавателя (1-5) - вводится вручную или через опросы
    /// </summary>
    public decimal TeacherRating { get; set; }

    /// <summary>
    /// Количество отзывов студентов - вводится вручную или через систему отзывов
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Средняя оценка по всем курсам - заполняется через сервис
    /// </summary>
    public decimal AverageGrade { get; set; }

    /// <summary>
    /// Средняя посещаемость на курсах - заполняется через сервис
    /// </summary>
    public decimal AverageAttendance { get; set; }

    /// <summary>
    /// Средний балл по экзаменам - заполняется через сервис
    /// </summary>
    public decimal AverageExamScore { get; set; }

    // === NAVIGATION PROPERTIES ===
    
    /// <summary>
    /// Преподаватель
    /// </summary>
    public Teacher? Teacher { get; set; }

    /// <summary>
    /// Академический период
    /// </summary>
    public AcademicPeriod? AcademicPeriod { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===
    
    /// <summary>
    /// Общее количество курсов - вычисляется из экземпляров курсов
    /// </summary>
    public int TotalCourses => Teacher?.CourseInstances?
        .Count(ci => ci.AcademicPeriodUid == AcademicPeriodUid) ?? 0;

    /// <summary>
    /// Количество активных курсов - вычисляется из активных экземпляров
    /// </summary>
    public int ActiveCourses => Teacher?.CourseInstances?
        .Count(ci => ci.AcademicPeriodUid == AcademicPeriodUid && ci.Status == CourseStatus.Active) ?? 0;

    /// <summary>
    /// Общее количество студентов - вычисляется из записей на курсы
    /// </summary>
    public int TotalStudents => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
        .SelectMany(ci => ci.Enrollments)
        .Select(e => e.StudentUid)
        .Distinct()
        .Count() ?? 0;

    /// <summary>
    /// Количество активных студентов - вычисляется из активных записей
    /// </summary>
    public int ActiveStudents => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
        .SelectMany(ci => ci.Enrollments)
        .Where(e => e.Status == EnrollmentStatus.Enrolled)
        .Select(e => e.StudentUid)
        .Distinct()
        .Count() ?? 0;

    /// <summary>
    /// Общее количество заданий - вычисляется из assignments
    /// </summary>
    public int TotalAssignments => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
        .SelectMany(ci => ci.Assignments)
        .Count() ?? 0;

    /// <summary>
    /// Количество проверенных заданий - вычисляется из graded submissions
    /// </summary>
    public int GradedAssignments => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
        .SelectMany(ci => ci.Assignments)
        .SelectMany(a => a.Submissions)
        .Count(s => s.Status == SubmissionStatus.Graded) ?? 0;

    /// <summary>
    /// Количество ожидающих проверки заданий - вычисляется из pending submissions
    /// </summary>
    public int PendingAssignments => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
        .SelectMany(ci => ci.Assignments)
        .SelectMany(a => a.Submissions)
        .Count(s => s.Status == SubmissionStatus.Submitted) ?? 0;

    /// <summary>
    /// Количество проведенных экзаменов - вычисляется из exams
    /// </summary>
    public int TotalExams => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
        .SelectMany(ci => ci.Exams)
        .Count() ?? 0;

    /// <summary>
    /// Процент успешности студентов - отношение завершенных курсов к общему количеству
    /// </summary>
    public decimal StudentSuccessRate
    {
        get
        {
            var allEnrollments = Teacher?.CourseInstances?
                .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
                .SelectMany(ci => ci.Enrollments)
                .ToList();

            if (allEnrollments?.Any() != true)
                return 0m;

            var totalEnrollments = allEnrollments.Count;
            var completedEnrollments = allEnrollments.Count(e => e.Status == EnrollmentStatus.Completed);

            return totalEnrollments > 0 ? (decimal)completedEnrollments / totalEnrollments * 100 : 0m;
        }
    }

    /// <summary>
    /// Количество курированных групп - вычисляется из groups
    /// </summary>
    public int CuratedGroups => Teacher?.CuratorGroups?.Count ?? 0;

    /// <summary>
    /// Общая нагрузка преподавателя (в часах) - вычисляется из schedule slots
    /// </summary>
    public decimal TotalWorkloadHours
    {
        get
        {
            var scheduleSlots = Teacher?.CourseInstances?
                .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
                .SelectMany(ci => ci.ScheduleSlots)
                .ToList();

            if (scheduleSlots?.Any() != true)
                return 0m;

            return (decimal)scheduleSlots
                .Sum(ss => (ss.EndTime - ss.StartTime).TotalHours);
        }
    }

    /// <summary>
    /// Средняя нагрузка в неделю - вычисляется из schedule slots
    /// </summary>
    public decimal WeeklyWorkload
    {
        get
        {
            var scheduleSlots = Teacher?.CourseInstances?
                .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid)
                .SelectMany(ci => ci.ScheduleSlots)
                .Where(ss => ss.CourseInstance?.AcademicPeriod?.StartDate <= DateTime.UtcNow && 
                            ss.CourseInstance?.AcademicPeriod?.EndDate >= DateTime.UtcNow)
                .ToList();

            if (scheduleSlots?.Any() != true)
                return 0m;

            var hoursPerWeek = scheduleSlots
                .GroupBy(ss => ss.DayOfWeek)
                .Sum(g => g.Sum(ss => (decimal)(ss.EndTime - ss.StartTime).TotalHours));

            return hoursPerWeek;
        }
    }

    /// <summary>
    /// Количество активных предметов - вычисляется из уникальных subjects
    /// </summary>
    public int ActiveSubjects => Teacher?.CourseInstances?
        .Where(ci => ci.AcademicPeriodUid == AcademicPeriodUid && ci.Status == CourseStatus.Active)
        .Select(ci => ci.SubjectUid)
        .Distinct()
        .Count() ?? 0;

    /// <summary>
    /// Проверяет, активен ли преподаватель (имеет активные курсы)
    /// </summary>
    public bool IsActiveTeacher => ActiveCourses > 0;

    /// <summary>
    /// Получает эффективность преподавателя (комбинированный показатель)
    /// </summary>
    public decimal EfficiencyScore
    {
        get
        {
            // Простая формула эффективности на основе нескольких показателей
            var gradeScore = AverageGrade / 5m * 100; // Нормализуем оценки к 100
            var attendanceScore = AverageAttendance; // Уже в процентах
            var successScore = StudentSuccessRate; // Уже в процентах
            var ratingScore = TeacherRating / 5m * 100; // Нормализуем рейтинг к 100

            // Взвешенная средняя (можно настроить веса)
            return (gradeScore * 0.3m + attendanceScore * 0.25m + successScore * 0.25m + ratingScore * 0.2m);
        }
    }

    // === HELPER METHODS ===

    /// <summary>
    /// Принудительно обновляет кэшированные значения
    /// </summary>
    public void RefreshCalculatedFields()
    {
        LastCalculated = DateTime.UtcNow;
        // Здесь можно добавить логику для обновления кэшированных полей
        // если потребуется оптимизация производительности
    }

    /// <summary>
    /// Проверяет, нужно ли обновить статистику
    /// </summary>
    public bool ShouldRecalculate(TimeSpan maxAge)
    {
        return DateTime.UtcNow - LastCalculated > maxAge;
    }
} 