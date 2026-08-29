using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Analytics;

/// <summary>
/// Аналитика группы
/// </summary>
public class GroupAnalytics : AuditableEntity
{
    /// <summary>
    /// Идентификатор группы
    /// </summary>
    public Guid GroupUid { get; set; }

    // === STORED FIELDS (обновляются периодически) ===
    
    /// <summary>
    /// Время последнего обновления статистики
    /// </summary>
    public DateTime LastCalculated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Средний GPA группы - заполняется через сервис
    /// </summary>
    public decimal AverageGPA { get; set; }

    /// <summary>
    /// Процент посещаемости группы - заполняется через сервис
    /// </summary>
    public decimal AverageAttendance { get; set; }

    /// <summary>
    /// Средний балл по экзаменам - заполняется через сервис
    /// </summary>
    public decimal AverageExamScore { get; set; }

    /// <summary>
    /// Процент успешно сданных экзаменов - заполняется через сервис
    /// </summary>
    public decimal ExamPassRate { get; set; }

    /// <summary>
    /// Средний возраст студентов в группе - заполняется через сервис
    /// </summary>
    public decimal AverageAge { get; set; }

    /// <summary>
    /// Соотношение мужчин и женщин в группе (процент мужчин) - заполняется через сервис
    /// </summary>
    public decimal MalePercentage { get; set; }

    // === NAVIGATION PROPERTIES ===
    
    /// <summary>
    /// Группа
    /// </summary>
    public Group? Group { get; set; }

    // === COMPUTED PROPERTIES (автоматически вычисляются через связи) ===
    
    /// <summary>
    /// Общее количество студентов в группе - вычисляется из students
    /// </summary>
    public int TotalStudents => Group?.Students?.Count ?? 0;

    /// <summary>
    /// Количество активных студентов - вычисляется из активных students
    /// </summary>
    public int ActiveStudents => Group?.Students?
        .Count(s => s.Status == StudentStatus.Active) ?? 0;

    /// <summary>
    /// Количество выпускников - вычисляется из graduated students
    /// </summary>
    public int GraduatedStudents => Group?.Students?
        .Count(s => s.Status == StudentStatus.Graduated) ?? 0;

    /// <summary>
    /// Количество отчисленных студентов - вычисляется из suspended students
    /// </summary>
    public int SuspendedStudents => Group?.Students?
        .Count(s => s.Status == StudentStatus.Suspended) ?? 0;

    /// <summary>
    /// Количество курсов у группы - вычисляется из course instances
    /// </summary>
    public int TotalCourses
    {
        get
        {
            // Получаем курсы через enrollments студентов группы
            var courseInstanceUids = Group?.Students?
                .SelectMany(s => s.Enrollments)
                .Select(e => e.CourseInstanceUid)
                .Distinct()
                .ToList();

            return courseInstanceUids?.Count ?? 0;
        }
    }

    /// <summary>
    /// Количество активных курсов - вычисляется из active enrollments
    /// </summary>
    public int ActiveCourses
    {
        get
        {
            var activeCourseInstanceUids = Group?.Students?
                .SelectMany(s => s.Enrollments)
                .Where(e => e.Status == EnrollmentStatus.Enrolled)
                .Select(e => e.CourseInstanceUid)
                .Distinct()
                .ToList();

            return activeCourseInstanceUids?.Count ?? 0;
        }
    }

    /// <summary>
    /// Общее количество заданий - вычисляется из assignments курсов группы
    /// </summary>
    public int TotalAssignments
    {
        get
        {
            var assignmentUids = Group?.Students?
                .SelectMany(s => s.Enrollments)
                .SelectMany(e => e.CourseInstance?.Assignments ?? new List<Assignment>())
                .Select(a => a.Uid)
                .Distinct()
                .ToList();

            return assignmentUids?.Count ?? 0;
        }
    }

    /// <summary>
    /// Процент выполнения заданий - вычисляется из submissions
    /// </summary>
    public decimal AssignmentCompletionRate
    {
        get
        {
            if (TotalAssignments == 0 || ActiveStudents == 0)
                return 0m;

            var totalPossibleSubmissions = TotalAssignments * ActiveStudents;
            var actualSubmissions = Group?.Students?
                .SelectMany(s => s.Submissions)
                .Count(s => s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.Graded) ?? 0;

            return totalPossibleSubmissions > 0 ? (decimal)actualSubmissions / totalPossibleSubmissions * 100 : 0m;
        }
    }

    /// <summary>
    /// Количество завершенных курсов группой - вычисляется из completed enrollments
    /// </summary>
    public int CompletedCourses
    {
        get
        {
            var completedCourseInstanceUids = Group?.Students?
                .SelectMany(s => s.Enrollments)
                .Where(e => e.Status == EnrollmentStatus.Completed)
                .Select(e => e.CourseInstanceUid)
                .Distinct()
                .ToList();

            return completedCourseInstanceUids?.Count ?? 0;
        }
    }

    /// <summary>
    /// Процент успешности группы - отношение завершенных курсов к общему количеству
    /// </summary>
    public decimal SuccessRate
    {
        get
        {
            if (TotalCourses == 0)
                return 0m;
            
            return (decimal)CompletedCourses / TotalCourses * 100;
        }
    }

    /// <summary>
    /// Текущий курс группы - заполняется через сервис или устанавливается по умолчанию
    /// </summary>
    public int CurrentCourse { get; set; } = 1;

    /// <summary>
    /// Год поступления группы - заполняется через сервис или устанавливается по умолчанию
    /// </summary>
    public int AdmissionYear { get; set; } = DateTime.UtcNow.Year;

    /// <summary>
    /// Куратор группы - получается через navigation property
    /// </summary>
    public Teacher? Curator => Group?.Curator;

    /// <summary>
    /// Проверяет, активна ли группа
    /// </summary>
    public bool IsActive => Group?.IsActive ?? false;

    /// <summary>
    /// Общий рейтинг группы (комбинированный показатель)
    /// </summary>
    public decimal OverallRating
    {
        get
        {
            // Простая формула рейтинга группы
            var gpaScore = AverageGPA / 5m * 100; // Нормализуем GPA к 100
            var attendanceScore = AverageAttendance; // Уже в процентах
            var completionScore = AssignmentCompletionRate; // Уже в процентах
            var examScore = ExamPassRate; // Уже в процентах

            // Взвешенная средняя
            return gpaScore * 0.3m + attendanceScore * 0.25m + completionScore * 0.25m + examScore * 0.2m;
        }
    }

    /// <summary>
    /// Количество студентов по статусам
    /// </summary>
    public Dictionary<StudentStatus, int> StudentsByStatus
    {
        get
        {
            var students = Group?.Students?.ToList() ?? new List<Student>();
            
            return Enum.GetValues<StudentStatus>()
                .ToDictionary(status => status, status => students.Count(s => s.Status == status));
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

    /// <summary>
    /// Получает топ студентов группы по GPA
    /// </summary>
    public IEnumerable<Student> GetTopStudents(int count = 5)
    {
        return Group?.Students?
            .Where(s => s.Status == StudentStatus.Active)
            .OrderByDescending(s => s.GPA)
            .Take(count) ?? Enumerable.Empty<Student>();
    }

    /// <summary>
    /// Получает студентов группы в зоне риска (низкая успеваемость)
    /// </summary>
    public IEnumerable<Student> GetAtRiskStudents(double minGPA = 2.5)
    {
        return Group?.Students?
            .Where(s => s.Status == StudentStatus.Active && s.GPA < minGPA)
            .OrderBy(s => s.GPA) ?? Enumerable.Empty<Student>();
    }
} 