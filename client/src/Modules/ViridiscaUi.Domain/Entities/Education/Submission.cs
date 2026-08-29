using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Domain.Entities.Education.Enums;

namespace ViridiscaUi.Domain.Entities.Education;

/// <summary>
/// Сданная студентом работа по заданию
/// </summary>
public class Submission : AuditableEntity
{
    /// <summary>
    /// Идентификатор студента
    /// </summary>
    public Guid StudentUid { get; set; }

    /// <summary>
    /// Идентификатор задания
    /// </summary>
    public Guid AssignmentUid { get; set; }

    /// <summary>
    /// Дата сдачи работы
    /// </summary>
    public DateTime? SubmissionDate { get; set; }

    /// <summary>
    /// Содержимое работы (текст ответа или ссылка на файл)
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Путь к файлу работы
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Дата сдачи работы (алиас для SubmissionDate)
    /// </summary>
    public DateTime? SubmittedAt 
    { 
        get => SubmissionDate; 
        set => SubmissionDate = value; 
    }

    /// <summary>
    /// Активна ли работа
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Оценка за работу
    /// </summary>
    public double? Score { get; set; }

    /// <summary>
    /// Комментарий преподавателя
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Идентификатор преподавателя, поставившего оценку
    /// </summary>
    public Guid? GradedByUid { get; set; }

    /// <summary>
    /// Дата выставления оценки
    /// </summary>
    public DateTime? GradedDate { get; set; }

    /// <summary>
    /// Дата выставления оценки (альтернативное название)
    /// </summary>
    public DateTime? GradedAt 
    { 
        get => GradedDate; 
        set => GradedDate = value; 
    }

    /// <summary>
    /// Статус работы
    /// </summary>
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    /// <summary>
    /// Студент, сдавший работу
    /// </summary>
    public Student? Student { get; set; }

    /// <summary>
    /// Задание, по которому сдана работа
    /// </summary>
    public Assignment? Assignment { get; set; }

    /// <summary>
    /// Преподаватель, поставивший оценку
    /// </summary>
    public Teacher? GradedBy { get; set; }

    /// <summary>
    /// Связанная оценка (если выставлена)
    /// </summary>
    public Grade? Grade { get; set; }

    /// <summary>
    /// Срок сдачи задания (из связанного задания)
    /// </summary>
    public DateTime? DueDate => Assignment?.DueDate;

    /// <summary>
    /// Создает новый экземпляр сданной работы
    /// </summary>
    public Submission()
    {
        Uid = Guid.NewGuid();
    }

    /// <summary>
    /// Создает новый экземпляр сданной работы с указанными параметрами
    /// </summary>
    public Submission(Guid studentUid, Guid assignmentUid, string content)
    {
        Uid = Guid.NewGuid();
        StudentUid = studentUid;
        AssignmentUid = assignmentUid;
        Content = content;
        SubmissionDate = DateTime.UtcNow;
        Status = SubmissionStatus.Submitted;
    }

    /// <summary>
    /// Отображаемый статус работы
    /// </summary>
    public string StatusDisplayName => Status switch
    {
        SubmissionStatus.Draft => "Черновик",
        SubmissionStatus.Submitted => "Сдано",
        SubmissionStatus.Late => "Сдано с опозданием",
        SubmissionStatus.UnderReview => "На проверке",
        SubmissionStatus.Graded => "Оценено",
        SubmissionStatus.Returned => "Возвращено на доработку",
        _ => "Неизвестный статус"
    };

    /// <summary>
    /// Проверяет, оценена ли работа
    /// </summary>
    public bool IsGraded => Status == SubmissionStatus.Graded && Score.HasValue;

    /// <summary>
    /// Проверяет, сдана ли работа с опозданием
    /// </summary>
    public bool IsLate => Status == SubmissionStatus.Late || 
                         (Assignment?.DueDate.HasValue == true && SubmissionDate > Assignment.DueDate.Value);
}
