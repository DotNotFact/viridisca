using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика курса
/// </summary>
public class CourseAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор экземпляра курса
    /// </summary>
    public Guid CourseInstanceUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===
    
    /// <summary>
    /// Время последнего обновления статистики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Уровень вовлеченности студентов (0-100) - может вычисляться сложным алгоритмом
    /// </summary>
    public decimal EngagementLevel { get; set; }

    /// <summary>
    /// Средний балл по курсу - заполняется через сервис
    /// </summary>
    public decimal AverageGrade { get; set; }

    /// <summary>
    /// Средняя посещаемость - заполняется через сервис
    /// </summary>
    public decimal AverageAttendance { get; set; }

    /// <summary>
    /// Средний балл по тестам - заполняется через сервис
    /// </summary>
    public decimal AverageQuizScore { get; set; }

    /// <summary>
    /// Средний балл по экзаменам - заполняется через сервис
    /// </summary>
    public decimal AverageExamScore { get; set; }

    /// <summary>
    /// Процент сдачи экзаменов - заполняется через сервис
    /// </summary>
    public decimal ExamPassRate { get; set; }

    // === NAVIGATION PROPERTIES ===
    
    /// <summary>
    /// Экземпляр курса
    /// </summary>
    public CourseInstance? CourseInstance { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===
    
    /// <summary>
    /// Общее количество записанных студентов - вычисляется из enrollments
    /// </summary>
    public int TotalEnrollments => CourseInstance?.Enrollments?.Count ?? 0;

    /// <summary>
    /// Количество активных студентов - вычисляется из активных enrollments
    /// </summary>
    public int ActiveStudents => CourseInstance?.Enrollments?
        .Count(e => e.Status == EnrollmentStatus.Enrolled) ?? 0;

    /// <summary>
    /// Количество завершивших курс - вычисляется из completed enrollments
    /// </summary>
    public int CompletedStudents => CourseInstance?.Enrollments?
        .Count(e => e.Status == EnrollmentStatus.Completed) ?? 0;

    /// <summary>
    /// Количество отчисленных студентов - вычисляется из dropped enrollments
    /// </summary>
    public int DroppedStudents => CourseInstance?.Enrollments?
        .Count(e => e.Status == EnrollmentStatus.Dropped) ?? 0;

    /// <summary>
    /// Процент успеваемости - отношение завершенных к общему количеству
    /// </summary>
    public decimal PassRate
    {
        get
        {
            if (TotalEnrollments == 0)
                return 0m;
            
            return (decimal)CompletedStudents / TotalEnrollments * 100;
        }
    }

    /// <summary>
    /// Количество заданий в курсе - вычисляется из assignments
    /// </summary>
    public int TotalAssignments => CourseInstance?.Assignments?.Count ?? 0;

    /// <summary>
    /// Средний процент выполнения заданий - вычисляется из submissions
    /// </summary>
    public decimal AssignmentCompletionRate
    {
        get
        {
            if (TotalAssignments == 0 || ActiveStudents == 0)
                return 0m;

            var totalPossibleSubmissions = TotalAssignments * ActiveStudents;
            var actualSubmissions = CourseInstance?.Assignments?
                .SelectMany(a => a.Submissions)
                .Count(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Graded) ?? 0;

            return totalPossibleSubmissions > 0 ? (decimal)actualSubmissions / totalPossibleSubmissions * 100 : 0m;
        }
    }

    /// <summary>
    /// Количество тестов в курсе - вычисляется из quizzes
    /// </summary>
    public int TotalQuizzes => CourseInstance?.Quizzes?.Count ?? 0;

    /// <summary>
    /// Количество обсуждений - вычисляется из discussions
    /// </summary>
    public int TotalDiscussions => CourseInstance?.Discussions?.Count ?? 0;

    /// <summary>
    /// Количество сообщений в обсуждениях - вычисляется из discussion posts
    /// </summary>
    public int TotalDiscussionPosts => CourseInstance?.Discussions?
        .SelectMany(d => d.Posts)
        .Count() ?? 0;

    /// <summary>
    /// Количество экзаменов в курсе - вычисляется из exams
    /// </summary>
    public int TotalExams => CourseInstance?.Exams?.Count ?? 0;

    /// <summary>
    /// Процент отсева - отношение отчисленных к общему количеству
    /// </summary>
    public decimal DropoutRate
    {
        get
        {
            if (TotalEnrollments == 0)
                return 0m;
            
            return (decimal)DroppedStudents / TotalEnrollments * 100;
        }
    }

    /// <summary>
    /// Активность курса - процент активных студентов
    /// </summary>
    public decimal ActivityRate
    {
        get
        {
            if (TotalEnrollments == 0)
                return 0m;
            
            return (decimal)ActiveStudents / TotalEnrollments * 100;
        }
    }

    /// <summary>
    /// Статус курса
    /// </summary>
    public CourseStatus Status => CourseInstance?.Status ?? CourseStatus.Draft;

    /// <summary>
    /// Проверяет, активен ли курс
    /// </summary>
    public bool IsActive => Status == CourseStatus.Active;

    /// <summary>
    /// Общий рейтинг курса (комбинированный показатель)
    /// </summary>
    public decimal OverallRating
    {
        get
        {
            // Простая формула рейтинга на основе нескольких показателей
            var gradeScore = AverageGrade / 5m * 100; // Нормализуем оценки к 100
            var attendanceScore = AverageAttendance; // Уже в процентах
            var completionScore = AssignmentCompletionRate; // Уже в процентах
            var passScore = PassRate; // Уже в процентах

            // Взвешенная средняя
            return gradeScore * 0.3m + attendanceScore * 0.25m + completionScore * 0.25m + passScore * 0.2m;
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