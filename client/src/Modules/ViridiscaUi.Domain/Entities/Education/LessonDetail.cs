namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Детальная информация о занятии для отображения в UI
/// </summary>
public class LessonDetail
{
    /// <summary>
    /// Идентификатор урока
    /// </summary>
    public Guid LessonUid { get; set; }

    /// <summary>
    /// Тема урока
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание урока
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Время начала урока
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Время окончания урока
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Имя преподавателя
    /// </summary>
    public string TeacherFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия преподавателя
    /// </summary>
    public string TeacherLastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество преподавателя
    /// </summary>
    public string TeacherMiddleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;

    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// Название аудитории
    /// </summary>
    public string Room { get; set; } = string.Empty;
    
    /// <summary>
    /// Признак отмены урока
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Признак завершения урока
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Полное имя преподавателя
    /// </summary>
    public string TeacherFullName => $"{TeacherLastName} {TeacherFirstName} {TeacherMiddleName}".Trim();

    /// <summary>
    /// Продолжительность занятия
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Статус занятия
    /// </summary>
    public string Status
    {
        get
        {
            if (IsCancelled) return "Отменено";
            if (IsCompleted) return "Завершено";
            if (DateTime.Now > EndTime) return "Пропущено";
            if (DateTime.Now >= StartTime && DateTime.Now <= EndTime) return "Идет";
            return "Запланировано";
        }
    }

    /// <summary>
    /// Создает новый экземпляр детальной информации об уроке
    /// </summary>
    public LessonDetail()
    {
    }

    /// <summary>
    /// Создает новый экземпляр детальной информации об уроке с указанными параметрами
    /// </summary>
    public LessonDetail(
        Guid lessonUid,
        string title,
        string description,
        DateTime startTime,
        DateTime endTime,
        string teacherLastName,
        string teacherFirstName,
        string teacherMiddleName,
        string subjectName,
        string groupName,
        string room,
        bool isCancelled,
        bool isCompleted)
    {
        LessonUid = lessonUid;
        Title = title ?? string.Empty;
        Description = description ?? string.Empty;
        StartTime = startTime;
        EndTime = endTime;
        TeacherLastName = teacherLastName ?? string.Empty;
        TeacherFirstName = teacherFirstName ?? string.Empty;
        TeacherMiddleName = teacherMiddleName ?? string.Empty;
        SubjectName = subjectName ?? string.Empty;
        GroupName = groupName ?? string.Empty;
        Room = room ?? string.Empty;
        IsCancelled = isCancelled;
        IsCompleted = isCompleted;
    }

    /// <summary>
    /// Создает пустой объект LessonDetail для случаев, когда данные недоступны
    /// </summary>
    public static LessonDetail CreateEmpty()
    {
        return new LessonDetail
        {
            LessonUid = Guid.Empty,
            Title = "Занятие не найдено",
            Description = "Информация о занятии недоступна",
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MinValue,
            TeacherLastName = "Неизвестно",
            TeacherFirstName = string.Empty,
            TeacherMiddleName = string.Empty,
            SubjectName = "Неизвестный предмет",
            GroupName = "Неизвестная группа",
            Room = string.Empty,
            IsCancelled = false,
            IsCompleted = false
        };
    }
}
