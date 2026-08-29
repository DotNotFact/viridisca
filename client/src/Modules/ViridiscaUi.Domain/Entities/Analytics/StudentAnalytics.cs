using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика успеваемости студента
/// </summary>
public class StudentAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid { get; set; }

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
    /// Время, проведенное в системе (в минутах)
    /// </summary>
    public int TimeSpentMinutes { get; set; }

    /// <summary>
    /// Дата последней активности
    /// </summary>
    public DateTime? LastActivity { get; set; }

    /// <summary>
    /// Процент посещаемости - заполняется через сервис
    /// </summary>
    public decimal AttendancePercentage { get; set; }

    // === NAVIGATION PROPERTIES ===
    
    /// <summary>
    /// Студент
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Академический период
    /// </summary>
    public AcademicPeriod? AcademicPeriod { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===
    
    /// <summary>
    /// Средний балл (GPA) - вычисляется из оценок студента за период
    /// </summary>
    public decimal GPA => Student?.Grades
        .Where(g => g.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Where(g => g.Value > 0) // Фильтруем нулевые оценки
        .Average(g => g.Value) ?? 0m;

    /// <summary>
    /// Общее количество кредитов - вычисляется из записей на курсы
    /// </summary>
    public int TotalCredits => Student?.Enrollments
        .Where(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Where(e => e.Status == EnrollmentStatus.Enrolled)
        .Sum(e => e.CourseInstance?.Subject?.Credits ?? 0) ?? 0;

    /// <summary>
    /// Количество завершенных курсов - вычисляется из записей
    /// </summary>
    public int CompletedCourses => Student?.Enrollments
        .Where(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;

    /// <summary>
    /// Количество проваленных курсов - вычисляется из записей
    /// </summary>
    public int FailedCourses => Student?.Enrollments
        .Where(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Count(e => e.Status == EnrollmentStatus.Failed) ?? 0;

    /// <summary>
    /// Количество сданных заданий - вычисляется из submissions
    /// </summary>
    public int SubmittedAssignments => Student?.Submissions
        .Where(s => s.Assignment?.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Where(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Graded)
        .Count() ?? 0;

    /// <summary>
    /// Количество просроченных заданий - вычисляется из assignments и submissions
    /// </summary>
    public int OverdueAssignments
    {
        get
        {
            var studentEnrollments = Student?.Enrollments
                .Where(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
                .Select(e => e.CourseInstanceUid)
                .ToList() ?? new List<Guid>();

            if (!studentEnrollments.Any())
                return 0;

            // Получаем все задания студента за период
            var assignments = Student?.Enrollments
                .Where(e => studentEnrollments.Contains(e.CourseInstanceUid))
                .SelectMany(e => e.CourseInstance?.Assignments ?? new List<Assignment>())
                .Where(a => a.DueDate.HasValue && a.DueDate < DateTime.UtcNow)
                .ToList() ?? new List<Assignment>();

            // Проверяем, какие из них не сданы или сданы с опозданием
            var overdueCount = 0;
            foreach (var assignment in assignments)
            {
                var submission = Student?.Submissions
                    .FirstOrDefault(s => s.AssignmentUid == assignment.Uid);
                
                if (submission == null || 
                    assignment.DueDate.HasValue && submission.SubmittedAt > assignment.DueDate)
                {
                    overdueCount++;
                }
            }

            return overdueCount;
        }
    }

    /// <summary>
    /// Средний балл по тестам - вычисляется из quiz attempts
    /// </summary>
    public decimal AverageQuizScore => Student?.QuizAttempts
        .Where(qa => qa.Quiz?.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Where(qa => qa.Score.HasValue)
        .Average(qa => (decimal)qa.Score!.Value) ?? 0m;

    /// <summary>
    /// Количество пройденных тестов - вычисляется из quiz attempts
    /// </summary>
    public int CompletedQuizzes => Student?.QuizAttempts
        .Where(qa => qa.Quiz?.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Where(qa => qa.CompletedAt.HasValue)
        .Count() ?? 0;

    /// <summary>
    /// Количество активных курсов - вычисляется из enrollments
    /// </summary>
    public int ActiveCourses => Student?.Enrollments
        .Where(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
        .Count(e => e.Status == EnrollmentStatus.Enrolled) ?? 0;

    /// <summary>
    /// Количество заданий, ожидающих выполнения
    /// </summary>
    public int PendingAssignments
    {
        get
        {
            var studentEnrollments = Student?.Enrollments
                .Where(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid)
                .Select(e => e.CourseInstanceUid)
                .ToList() ?? new List<Guid>();

            if (!studentEnrollments.Any())
                return 0;

            // Получаем все активные задания студента
            var assignments = Student?.Enrollments
                .Where(e => studentEnrollments.Contains(e.CourseInstanceUid))
                .SelectMany(e => e.CourseInstance?.Assignments ?? new List<Assignment>())
                .Where(a => a.Status == AssignmentStatus.Published)
                .Where(a => !a.DueDate.HasValue || a.DueDate > DateTime.UtcNow)
                .ToList() ?? new List<Assignment>();

            // Считаем те, которые еще не сданы
            var pendingCount = 0;
            foreach (var assignment in assignments)
            {
                var hasSubmission = Student?.Submissions
                    .Any(s => s.AssignmentUid == assignment.Uid && 
                             (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Graded)) ?? false;
                
                if (!hasSubmission)
                {
                    pendingCount++;
                }
            }

            return pendingCount;
        }
    }

    /// <summary>
    /// Процент успешности - отношение завершенных курсов к общему количеству
    /// </summary>
    public decimal SuccessRate
    {
        get
        {
            var totalEnrollments = Student?.Enrollments
                .Count(e => e.CourseInstance?.AcademicPeriodUid == AcademicPeriodUid) ?? 0;
            
            if (totalEnrollments == 0)
                return 0m;
            
            return (decimal)CompletedCourses / totalEnrollments * 100;
        }
    }

    /// <summary>
    /// Текущий статус студента в академическом периоде
    /// </summary>
    public StudentStatus CurrentStatus => Student?.Status ?? StudentStatus.Inactive;

    /// <summary>
    /// Проверяет, активен ли студент в данном периоде
    /// </summary>
    public bool IsActiveInPeriod => ActiveCourses > 0 && CurrentStatus == StudentStatus.Active;

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
    /// Проверяет, нужно ли обновить статистику (например, если прошло более часа)
    /// </summary>
    public bool ShouldRecalculate(TimeSpan maxAge)
    {
        return DateTime.UtcNow - LastCalculated > maxAge;
    }
} 