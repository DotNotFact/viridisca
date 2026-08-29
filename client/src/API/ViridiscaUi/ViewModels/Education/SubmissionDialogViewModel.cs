using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования работы студента
/// </summary>
public class SubmissionDialogViewModel : RoutableViewModelBase
{
    #region Fields
     
    private readonly Submission? _originalSubmission;

    #endregion

    #region Properties

    [Reactive] public string Content { get; set; } = string.Empty;
    [Reactive] public string FilePath { get; set; } = string.Empty;
    [Reactive] public SubmissionStatus Status { get; set; }
    [Reactive] public DateTime? SubmittedAt { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public bool IsLate { get; set; }
    [Reactive] public double? Grade { get; set; }
    [Reactive] public string Feedback { get; set; } = string.Empty;
    [Reactive] public string ValidationError { get; set; } = string.Empty;

    [Reactive] public Assignment? Assignment { get; set; }
    [Reactive] public Student? Student { get; set; }

    public ObservableCollection<Assignment> AvailableAssignments { get; } = new();
    public ObservableCollection<Student> AvailableStudents { get; } = new();

    #endregion

    #region Commands

    public ReactiveCommand<Unit, Submission?> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    #endregion

    #region Constructor

    public SubmissionDialogViewModel( 
        Submission? submission = null,
        string? dialogTitle = null)
        : base(null)
    { 
        _originalSubmission = submission;

        // Инициализация значений по умолчанию
        Status = SubmissionStatus.Draft;
        SubmittedAt = DateTime.Now;
        IsActive = true;

        // Команды
        var canSave = this.WhenAnyValue(
            x => x.Content,
            x => x.Assignment,
            x => x.Student,
            x => x.ValidationError,
            (content, assignment, student, error) =>
                !string.IsNullOrWhiteSpace(content) &&
                assignment != null &&
                student != null &&
                string.IsNullOrEmpty(error));

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
        CancelCommand = ReactiveCommand.Create(() => { });

        // Валидация при изменении свойств
        this.WhenAnyValue(
                x => x.Content,
                x => x.Assignment,
                x => x.Student,
                x => x.SubmittedAt,
                x => x.Grade)
            .Subscribe(_ => Validate())
            .DisposeWith(Disposables);

        // Загрузка доступных данных
        LoadAvailableDataAsync().ConfigureAwait(false);

        // Инициализация из существующей работы
        if (submission != null)
        {
            InitializeFromEntity(submission);
        }
    }

    #endregion

    #region Methods

    private void InitializeFromEntity(Submission entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Content = entity.Content;
        FilePath = entity.FilePath;
        Status = entity.Status;
        SubmittedAt = entity.SubmittedAt;
        IsActive = entity.IsActive;
        IsLate = entity.IsLate;
        Grade = entity.Score.HasValue ? (double)entity.Score.Value : null;
        Feedback = entity.Feedback;
        
        // Загружаем связанные сущности
        if (entity.Assignment != null)
            Assignment = AvailableAssignments.FirstOrDefault(a => a.Uid == entity.Assignment.Uid);
        if (entity.Student != null)
            Student = AvailableStudents.FirstOrDefault(s => s.Uid == entity.Student.Uid);
    }

    private void Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Content))
            errors.Add("Содержание работы обязательно для заполнения");

        if (Assignment == null)
            errors.Add("Необходимо выбрать задание");

        if (Student == null)
            errors.Add("Необходимо выбрать студента");

        if (SubmittedAt.HasValue && SubmittedAt > DateTime.Now)
            errors.Add("Дата сдачи не может быть в будущем");

        if (Grade.HasValue && (Grade < 0 || Grade > 5))
            errors.Add("Оценка должна быть от 0 до 5");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
    }

    private async Task<Submission?> SaveAsync()
    {
        try
        {
            var entity = _originalSubmission ?? new Submission();
            
            entity.Content = Content;
            entity.FilePath = FilePath;
            entity.Status = Status;
            entity.SubmittedAt = SubmittedAt ?? DateTime.UtcNow;
            entity.IsActive = IsActive;
            entity.Grade = Grade.HasValue ? new Grade { Value = (decimal)Grade.Value } : null;
            entity.Feedback = Feedback;
            entity.AssignmentUid = Assignment?.Uid ?? Guid.Empty;
            entity.StudentUid = Student?.Uid ?? Guid.Empty;
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
            Logger?.LogError(ex, "Ошибка при сохранении работы студента");
            throw;
        }
    }

    private async Task LoadAvailableDataAsync()
    {
        try
        {
            // TODO: Загрузка заданий и студентов через соответствующие сервисы
            // Пока что заглушка
            await Task.Delay(1);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Ошибка при загрузке данных для работы студента");
            throw;
        }
    }

    #endregion
} 

