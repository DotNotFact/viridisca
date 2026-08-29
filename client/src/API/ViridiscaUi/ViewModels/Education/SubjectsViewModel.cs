using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System.Enums;
using Microsoft.EntityFrameworkCore; 
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления предметами
/// </summary>
[Route("subjects", 
    DisplayName = "Предметы", 
    IconKey = "BookMultiple", 
    Order = 5,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление учебными предметами")]
public class SubjectsViewModel : RoutableViewModelBase
{
    private readonly ISubjectService _subjectService;
    private readonly IDepartmentService _departmentService; 
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    [Reactive] public ObservableCollection<SubjectViewModel> Subjects { get; set; } = new();
    [Reactive] public SubjectViewModel? SelectedSubject { get; set; }
    [Reactive] public string SearchTerm { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }

    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalSubjects { get; set; }

    // Computed properties
    public bool HasSelectedSubject => SelectedSubject != null;

    // Команды
    public ReactiveCommand<Unit, Unit> LoadSubjectsCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> CreateSubjectCommand { get; private set; }
    public ReactiveCommand<SubjectViewModel, Unit> EditSubjectCommand { get; private set; }
    public ReactiveCommand<SubjectViewModel, Unit> DeleteSubjectCommand { get; private set; }
    public ReactiveCommand<SubjectViewModel, Unit> ViewSubjectDetailsCommand { get; private set; }
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; }
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> FirstPageCommand { get; private set; }
    public ReactiveCommand<Unit, Unit> LastPageCommand { get; private set; }

    /// <summary>
    /// Количество предметов (алиас для совместимости)
    /// </summary>
    public int SubjectCount => Subjects.Count;

    /// <summary>
    /// Общее количество записей (алиас для совместимости)
    /// </summary>
    public int TotalCount => TotalSubjects;

    /// <summary>
    /// Название предмета
    /// </summary>
    [Reactive] public string SubjectName { get; set; } = string.Empty;

    /// <summary>
    /// UID предмета
    /// </summary>
    [Reactive] public Guid SubjectUid { get; set; }

    /// <summary>
    public SubjectsViewModel(
        IScreen hostScreen,
        ISubjectService subjectService,
        IDepartmentService departmentService, 
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        IDialogService dialogService)
        : base(hostScreen)
    {
        _subjectService = subjectService ?? throw new ArgumentNullException(nameof(subjectService));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService)); 
        _statusService = statusService ?? throw new ArgumentNullException(nameof(statusService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _permissionService = permissionService ?? throw new ArgumentNullException(nameof(permissionService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));

        InitializeCommands();
        SetupSubscriptions();
    }

    private void InitializeCommands()
    {
        LoadSubjectsCommand = CreateCommand(async () => await LoadSubjectsAsync(), null, "Ошибка загрузки предметов");
        RefreshCommand = CreateCommand(async () => await RefreshAsync(), null, "Ошибка обновления данных");
        CreateSubjectCommand = CreateCommand(async () => await CreateSubjectAsync(), null, "Ошибка создания предмета");
        EditSubjectCommand = CreateCommand<SubjectViewModel>(async (subject) => await EditSubjectAsync(subject), null, "Ошибка редактирования предмета");
        DeleteSubjectCommand = CreateCommand<SubjectViewModel>(async (subject) => await DeleteSubjectAsync(subject), null, "Ошибка удаления предмета");
        ViewSubjectDetailsCommand = CreateCommand<SubjectViewModel>(async (subject) => await ViewSubjectDetailsAsync(subject), null, "Ошибка просмотра деталей предмета");
        SearchCommand = CreateCommand<string>(async (searchTerm) => await SearchSubjectsAsync(searchTerm), null, "Ошибка поиска предметов");
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page), null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");
        FirstPageCommand = CreateCommand(async () => await FirstPageAsync(), null, "Ошибка перехода на первую страницу");
        LastPageCommand = CreateCommand(async () => await LastPageAsync(), null, "Ошибка перехода на последнюю страницу");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchTerm => SearchCommand.Execute(searchTerm ?? string.Empty).Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SelectedSubject)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedSubject)))
            .DisposeWith(Disposables);
    }

    private async Task LoadSubjectsAsync()
    {
        // Предотвращаем множественные одновременные вызовы
        if (IsLoading) return;
        
        LogInfo($"Loading subjects with search term: {SearchTerm}");
        IsLoading = true;
        ShowInfo("Загрузка предметов...");

        try
        {
            var (subjects, totalCount) = await _subjectService.GetPagedAsync(CurrentPage, PageSize, SearchTerm);
            
            Subjects.Clear();
            foreach (var subject in subjects)
            {
                Subjects.Add(new SubjectViewModel(subject));
            }

            TotalSubjects = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            LogInfo($"Loaded {SubjectCount} subjects, total: {TotalCount}");
            ShowSuccess($"Загружено {Subjects.Count} предметов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load subjects");
            ShowError("Не удалось загрузить список предметов");
            Subjects.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing subjects data");
        IsRefreshing = true;
        
        await LoadSubjectsAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создание нового предмета с расширенной валидацией
    /// </summary>
    private async Task CreateSubjectAsync()
    {
        LogInfo("Creating new subject");
        
        // Проверка прав доступа
        if (!await HasPermissionAsync("Subjects.Create"))
        {
            ShowError("У вас нет прав для создания предметов");
            return;
        }

        try
        {
            var newSubject = new Subject
            {
                Uid = Guid.NewGuid(),
                Name = string.Empty,
                Code = string.Empty,
                Description = string.Empty,
                IsActive = true,
                Credits = 3,
                LessonsPerWeek = 2,
                CreatedAt = DateTime.UtcNow
            };

            var dialogResult = await _dialogService.ShowSubjectEditDialogAsync(newSubject);
            if (dialogResult == null)
            {
                LogDebug("Subject creation cancelled by user");
                return;
            }

            // Валидация данных предмета
            var validationResult = await ValidateSubjectAsync(dialogResult);
            if (!validationResult.IsValid)
            {
                await _dialogService.ShowValidationErrorsAsync("Ошибки валидации", validationResult.Errors);
                return;
            }

            // Создание предмета
            var createdSubject = await _subjectService.CreateAsync(dialogResult);
            
            // Обновление UI
            Subjects.Add(new SubjectViewModel(createdSubject));
            TotalSubjects++;
            
            // Автоматическое обновление статистики
            await UpdateStatisticsAsync();
            
            LogInfo($"Subject created successfully: {SubjectName}");
            await _notificationService.SendNotificationAsync(
                "Предмет создан",
                $"Новый предмет '{createdSubject.Name}' был создан");

            ShowSuccess($"Предмет '{createdSubject.Name}' создан");
        }
        catch (InvalidOperationException ex)
        {
            LogError(ex, "Duplicate subject code");
            ErrorMessage = "Предмет с таким кодом уже существует";
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create subject");
            ShowError("Не удалось создать предмет. Попробуйте еще раз.");
        }
    }

    /// <summary>
    /// Редактирование предмета с optimistic locking
    /// </summary>
    private async Task EditSubjectAsync(SubjectViewModel subjectVm)
    {
        if (subjectVm == null) return;

        LogInfo($"Editing subject: {SubjectName}");

        // Проверка прав доступа
        if (!await HasPermissionAsync("Subjects.Update"))
        {
            ShowError("У вас нет прав для редактирования предметов");
            return;
        }

        try
        {
            // Получение актуальных данных с проверкой версии
            var subject = await _subjectService.GetByUidAsync(subjectVm.Uid);
            if (subject == null)
            {
                ShowError("Предмет не найден");
                await LoadSubjectsAsync(); // Обновляем список
                return;
            }

            // Проверка optimistic locking
            if (subject.LastModifiedAt != subjectVm.LastModifiedAt)
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Конфликт версий",
                    "Предмет был изменен другим пользователем. Хотите перезаписать изменения?");
                
                if (result == false)
                {
                    await LoadSubjectsAsync(); // Обновляем список
                    return;
                }
            }

            var dialogResult = await _dialogService.ShowSubjectEditDialogAsync(subject);
            if (dialogResult == null)
            {
                LogDebug("Subject editing cancelled by user");
                return;
            }

            // Валидация изменений
            var validationResult = await ValidateSubjectAsync(dialogResult);
            if (!validationResult.IsValid)
            {
                await _dialogService.ShowValidationErrorsAsync("Ошибки валидации", validationResult.Errors);
                return;
            }

            // Обновление предмета
            var updatedSubject = await _subjectService.UpdateAsync(dialogResult);
            
            // Обновление UI
            var index = Subjects.IndexOf(subjectVm);
            if (index >= 0)
            {
                Subjects[index] = new SubjectViewModel(updatedSubject);
                if (SelectedSubject?.Uid == updatedSubject.Uid)
                {
                    SelectedSubject = Subjects[index];
                }
            }
            
            // Автоматическое обновление статистики
            await UpdateStatisticsAsync();
            
            LogInfo($"Subject updated successfully: {SubjectName}");
            ShowSuccess($"Предмет '{updatedSubject.Name}' успешно обновлен");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            ShowError($"Конфликт одновременного редактирования: {ex.Message}");
            LogError(ex, "Concurrency conflict while updating subject");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update subject");
            ShowError("Не удалось обновить предмет. Попробуйте еще раз.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Удаление предмета с проверкой связанных данных
    /// </summary>
    private async Task DeleteSubjectAsync(SubjectViewModel subjectVm)
    {
        if (subjectVm == null) return;

        LogInfo($"Deleting subject: {SubjectName}");

        // Проверка прав доступа
        if (!await HasPermissionAsync("Subjects.Delete"))
        {
            ShowError("У вас нет прав для удаления предметов");
            return;
        }

        try
        {
            // Подтверждение удаления
            var relatedDataInfo = await CheckRelatedDataAsync(subjectVm.Uid);

            // Show confirmation dialog with proper string parameters
            var result = await _dialogService.ShowConfirmationDialogAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить предмет '{subjectVm.Name}'?");

            if (result == false) return;

            await _subjectService.DeleteAsync(subjectVm.Uid);
            await LoadSubjectsAsync(); // Обновляем список

            await _notificationService.SendNotificationAsync(
                $"Предмет '{subjectVm.Name}' был удален",
                "Предмет успешно удален из системы",
                NotificationType.Success);

            ShowSuccess($"Предмет '{subjectVm.Name}' удален");
            LogInfo($"Subject deleted successfully: {SubjectName}");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("foreign key") || ex.Message.Contains("связанных данных"))
        {
            var result = await _dialogService.ShowConfirmationDialogAsync(
                "Удаление предмета", 
                "Невозможно удалить предмет, так как он используется в курсах или учебных планах. Сначала удалите связанные записи.");
            
            LogWarning($"Foreign key constraint violation while deleting subject: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to delete subject");
            ShowError("Не удалось удалить предмет. Попробуйте еще раз.");
        }
    }

    private async Task ViewSubjectDetailsAsync(SubjectViewModel subjectVm)
    {
        if (subjectVm == null) return;

        LogInfo($"Viewing subject details: {SubjectName}");
        
        try
        {
            await NavigateToAsync($"subject-details/{subjectVm.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to subject details");
            ShowError("Не удалось открыть детали предмета");
        }
    }

    private async Task SearchSubjectsAsync(string searchTerm)
    {
        LogInfo($"Searching subjects with term: {SearchTerm}");
        CurrentPage = 1; // Сброс на первую страницу при поиске
        await LoadSubjectsAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        
        CurrentPage = page;
        await LoadSubjectsAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadSubjectsAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadSubjectsAsync();
        }
    }

    private async Task FirstPageAsync()
    {
        CurrentPage = 1;
        await LoadSubjectsAsync();
    }

    private async Task LastPageAsync()
    {
        CurrentPage = TotalPages;
        await LoadSubjectsAsync();
    }

    /// <summary>
    /// Валидация данных предмета
    /// </summary>
    private async Task<DomainValidationResult> ValidateSubjectAsync(Subject subject)
    {
        var result = new DomainValidationResult();

        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(subject.Name))
        {
            result.AddError("Name", "Название предмета обязательно для заполнения");
        }

        if (string.IsNullOrWhiteSpace(subject.Code))
        {
            result.AddError("Code", "Код предмета обязателен для заполнения");
        }

        if (subject.Credits <= 0)
        {
            result.AddError("Credits", "Количество кредитов должно быть больше нуля");
        }

        if (subject.LessonsPerWeek <= 0)
        {
            result.AddError("LessonsPerWeek", "Количество занятий в неделю должно быть больше нуля");
        }

        // Проверка уникальности кода предмета
        if (!string.IsNullOrWhiteSpace(subject.Code))
        {
            var existingSubject = await _subjectService.GetByCodeAsync(subject.Code);
            if (existingSubject != null && existingSubject.Uid != subject.Uid)
            {
                result.AddError("Code", $"Предмет с кодом '{subject.Code}' уже существует");
            }
        }

        // Проверка уникальности названия предмета
        if (!string.IsNullOrWhiteSpace(subject.Name))
        {
            var existingSubject = await _subjectService.GetByNameAsync(subject.Name);
            if (existingSubject != null && existingSubject.Uid != subject.Uid)
            {
                result.AddError("Name", $"Предмет с названием '{subject.Name}' уже существует");
            }
        }

        // Проверка валидности департамента
        if (subject.DepartmentUid.HasValue)
        {
            var department = await _departmentService.GetByUidAsync(subject.DepartmentUid.Value);
            if (department == null)
            {
                result.AddError("DepartmentUid", "Указанный департамент не найден");
            }
        }

        return result;
    }

    /// <summary>
    /// Проверка связанных данных перед удалением
    /// </summary>
    private async Task<SubjectRelatedDataInfo> CheckRelatedDataAsync(Guid subjectUid)
    {
        var relatedData = new SubjectRelatedDataInfo();

        try
        {
            // Проверяем связанные курсы
            var courses = await _subjectService.GetSubjectCoursesAsync(subjectUid);
            relatedData.CourseInstancesCount = courses?.Count() ?? 0;

            // Проверяем связанные учебные планы
            var curricula = await _subjectService.GetSubjectCurriculaAsync(subjectUid);
            relatedData.CurriculumsCount = curricula?.Count() ?? 0;

            // Проверяем связанные задания
            var assignments = await _subjectService.GetSubjectAssignmentsAsync(subjectUid);
            relatedData.AssignmentsCount = assignments?.Count() ?? 0;

            if (relatedData.HasRelatedData)
            {
                relatedData.RelatedDataDescriptions.Add($"Курсы: {relatedData.CourseInstancesCount}");
                relatedData.RelatedDataDescriptions.Add($"Учебные планы: {relatedData.CurriculumsCount}");
                relatedData.RelatedDataDescriptions.Add($"Задания: {relatedData.AssignmentsCount}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Error checking related data for subject {SubjectUid}");
        }

        return relatedData;
    }

    private async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            var currentPerson = await _authService.GetCurrentPersonAsync();
            if (currentPerson == null) return false;

            return await _permissionService.HasPermissionAsync(currentPerson.Uid, permission);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Error checking permission: {permission}");
            return false;
        }
    }

    private async Task UpdateStatisticsAsync()
    {
        try
        {
            // Обновление статистики предметов
            TotalSubjects = await _subjectService.GetTotalCountAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update statistics");
        }
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        await LoadSubjectsAsync();
    }
} 

