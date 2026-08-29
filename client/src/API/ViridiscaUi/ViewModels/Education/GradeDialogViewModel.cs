using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования оценки
/// </summary>
public class GradeDialogViewModel : RoutableViewModelBase
{
    private readonly Grade? _originalGrade;

    #region Properties

    [Reactive] public decimal Value { get; set; }
    [Reactive] public string Comment { get; set; } = string.Empty;
    [Reactive] public DateTime? GradedAt { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public string? ValidationError { get; set; }
    [Reactive] public string? Title { get; set; }

    /// <summary>
    /// Текущая редактируемая сущность
    /// </summary>
    [Reactive] public Grade? Entity { get; set; }

    /// <summary>
    /// UID оценки
    /// </summary>
    [Reactive] public Guid GradeUid { get; set; }

    /// <summary>
    /// UID студента
    /// </summary>
    [Reactive] public Guid StudentUid { get; set; }

    /// <summary>
    /// UID задания
    /// </summary>
    [Reactive] public Guid AssignmentUid { get; set; }

    /// <summary>
    /// UID преподавателя
    /// </summary>
    [Reactive] public Guid TeacherUid { get; set; }

    /// <summary>
    /// Максимальное значение оценки
    /// </summary>
    [Reactive] public decimal MaxValue { get; set; }

    [Reactive] public Assignment? Assignment { get; set; }
    [Reactive] public Student? Student { get; set; }
    [Reactive] public Teacher? Teacher { get; set; }

    public ObservableCollection<Assignment> AvailableAssignments { get; } = new();
    public ObservableCollection<Student> AvailableStudents { get; } = new();
    public ObservableCollection<Teacher> AvailableTeachers { get; } = new();

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Grade?> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    #endregion

    #region Constructor

    public GradeDialogViewModel(
        IScreen? hostScreen = null,
        Grade? grade = null)
        : base(hostScreen)
    {
        _originalGrade = grade;

        // decimal field initializers on [Reactive] properties corrupt ReactiveUI.Fody's
        // IL weaving (see TeacherEditorViewModel.HourlyRate) - assign defaults here instead.
        Value = 0;
        MaxValue = 100;
        GradedAt = DateTime.Now;
        IsActive = true;

        // Команды
        var canSave = this.WhenAnyValue(
            x => x.Value,
            x => x.Assignment,
            x => x.Student,
            x => x.Teacher,
            x => x.ValidationError,
            (value, assignment, student, teacher, error) =>
                value >= 0 && value <= 5 &&
                assignment != null &&
                student != null &&
                teacher != null &&
                string.IsNullOrEmpty(error));

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
        CancelCommand = ReactiveCommand.Create(() => { });

        // Настройка валидации
        this.WhenAnyValue(
                x => x.Value,
                x => x.Assignment,
                x => x.Student,
                x => x.Teacher,
                x => x.GradedAt)
            .Subscribe(_ => ValidateInternal())
            .DisposeWith(Disposables);

        // Инициализация из существующей оценки
        if (grade != null)
        {
            InitializeFromEntity(grade);
        }
        else
        {
            InitializeNew();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Инициализация ViewModel для создания новой оценки
    /// </summary>
    public async Task InitializeAsync()
    {
        InitializeNew();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Инициализация ViewModel для редактирования существующей оценки
    /// </summary>
    public async Task InitializeAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments)
    {
        InitializeFromEntity(grade);

        // Заполняем коллекции
        AvailableStudents.Clear();
        foreach (var student in students)
        {
            AvailableStudents.Add(student);
        }

        AvailableAssignments.Clear();
        foreach (var assignment in assignments)
        {
            AvailableAssignments.Add(assignment);
        }

        await Task.CompletedTask;
    }

    public void LoadGradeAsync(Grade grade)
    {
        if (grade == null) return;

        Entity = grade;
        GradeUid = grade.Uid;
        Value = grade.Value;
        MaxValue = grade.MaxValue ?? 0m;
        Comment = grade.Comment ?? string.Empty;
        GradedAt = grade.GradedAt;
        StudentUid = grade.StudentUid;
        AssignmentUid = grade.AssignmentUid ?? Guid.Empty;
        TeacherUid = grade.TeacherUid;

        IsEditMode = true;
        Title = "Редактирование оценки";
        LogDebug($"Loaded grade for editing: {GradeUid}");
    }

    public Grade? GetUpdatedGrade()
    {
        if (Entity == null) return null;

        Entity.Value = Value;
        Entity.MaxValue = MaxValue;
        Entity.Comment = Comment;
        Entity.GradedAt = GradedAt;
        Entity.StudentUid = StudentUid;
        Entity.AssignmentUid = AssignmentUid;
        Entity.TeacherUid = TeacherUid;

        return Entity;
    }

    #endregion

    #region Methods

    private void InitializeFromEntity(Grade entity)
    {
        Value = entity.Value;
        Comment = entity.Comment ?? string.Empty;
        GradedAt = entity.GradedAt;
        IsActive = entity.IsActive;

        Title = "Редактирование оценки";
    }

    private void InitializeNew()
    {
        Value = 0;
        Comment = string.Empty;
        GradedAt = DateTime.Now;
        IsActive = true;

        Title = "Создание оценки";
    }

    private async Task<Grade?> SaveAsync()
    {
        try
        {
            var entity = _originalGrade ?? new Grade();

            entity.Value = Value;
            entity.Comment = Comment;
            entity.GradedAt = GradedAt ?? DateTime.UtcNow;
            entity.IsActive = IsActive;

            if (Assignment != null)
                entity.AssignmentUid = Assignment.Uid;
            if (Student != null)
                entity.StudentUid = Student.Uid;
            if (Teacher != null)
                entity.TeacherUid = Teacher.Uid;

            entity.LastModifiedAt = DateTime.UtcNow;

            if (entity.Uid == Guid.Empty)
            {
                entity.Uid = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
            }

            return entity;
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Ошибка при сохранении оценки");
            throw;
        }
    }

    private void ValidateInternal()
    {
        var errors = new List<string>();

        if (Value < 0 || Value > 5)
            errors.Add("Оценка должна быть от 0 до 5");

        if (Assignment == null)
            errors.Add("Необходимо выбрать задание");

        if (Student == null)
            errors.Add("Необходимо выбрать студента");

        if (Teacher == null)
            errors.Add("Необходимо выбрать преподавателя");

        if (GradedAt.HasValue && GradedAt > DateTime.Now)
            errors.Add("Дата выставления оценки не может быть в будущем");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
    }

    #endregion
}

