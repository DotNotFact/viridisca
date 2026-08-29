using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.System.Enums;
using ViridiscaUi.ViewModels.System; 
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для управления учебными планами
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("curriculum",
    DisplayName = "Учебные планы",
    IconKey = "BookOpenPageVariant",
    Order = 10,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление учебными планами и программами")]
public class CurriculumViewModel : RoutableViewModelBase
{
    private readonly ICurriculumService _curriculumService;
    private readonly ISubjectService _subjectService;
    private readonly IDepartmentService _departmentService; 
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    // === СВОЙСТВА ===

    [Reactive] public ObservableCollection<CurriculumItemViewModel> Curricula { get; set; } = new();
    [Reactive] public CurriculumItemViewModel? SelectedCurriculum { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }

    // Фильтры
    [Reactive] public ObservableCollection<DepartmentViewModel> Departments { get; set; } = new();
    [Reactive] public DepartmentViewModel? SelectedDepartmentFilter { get; set; }
    [Reactive] public bool? IsActiveFilter { get; set; }
    [Reactive] public int? MinCreditsFilter { get; set; }
    [Reactive] public int? MaxCreditsFilter { get; set; }
    [Reactive] public int? AcademicYearFilter { get; set; }

    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 20;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalCurricula { get; set; }

    // Computed properties
    public bool HasSelectedCurriculum => SelectedCurriculum != null;
    [Reactive] public int SelectedCurriculaCount { get; set; }

    // === КОМАНДЫ ===

    public ReactiveCommand<Unit, Unit> LoadCurriculaCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CreateCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> EditCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> DeleteCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> ViewCurriculumDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> CopyCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> ActivateCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> DeactivateCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> ToggleActivationCommand { get; private set; } = null!;
    public ReactiveCommand<CurriculumItemViewModel, Unit> ExportCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ImportCurriculumCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> LoadStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkActivateCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeactivateCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> BulkDeleteCommand { get; private set; } = null!;

    // New properties
    [Reactive] public int CurriculumCount { get; set; }
    [Reactive] public int TotalCount { get; set; }
    [Reactive] public string CurriculumName { get; set; } = string.Empty;
    [Reactive] public Guid CurriculumUid { get; set; }

    public CurriculumViewModel(
        IScreen hostScreen,
        ICurriculumService curriculumService,
        ISubjectService subjectService,
        IDepartmentService departmentService, 
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        IDialogService dialogService) : base(hostScreen)
    {
        _curriculumService = curriculumService ?? throw new ArgumentNullException(nameof(curriculumService));
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
        LoadCurriculaCommand = CreateCommand(async () => await LoadCurriculaAsync());
        RefreshCommand = CreateCommand(async () => await RefreshAsync());
        CreateCurriculumCommand = CreateCommand(async () => await CreateCurriculumAsync(), null, "Ошибка создания учебного плана");
        EditCurriculumCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await EditCurriculumAsync(item));
        DeleteCurriculumCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await DeleteCurriculumAsync(item));
        ViewCurriculumDetailsCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await ViewCurriculumDetailsAsync(item), null, "Ошибка просмотра деталей учебного плана");
        CopyCurriculumCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await CopyCurriculumAsync(item), null, "Ошибка копирования учебного плана");
        ActivateCurriculumCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await ActivateCurriculumAsync(item), null, "Ошибка активации учебного плана");
        DeactivateCurriculumCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await DeactivateCurriculumAsync(item), null, "Ошибка деактивации учебного плана");
        ToggleActivationCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await ToggleActivationAsync(item), null, "Ошибка изменения статуса учебного плана");
        ExportCurriculumCommand = CreateCommand<CurriculumItemViewModel>(async (item) => await ExportCurriculumAsync(item), null, "Ошибка экспорта учебного плана");
        ImportCurriculumCommand = CreateCommand(async () => await ImportCurriculumAsync(), null, "Ошибка импорта учебного плана");
        LoadStatisticsCommand = CreateCommand(async () => await LoadStatisticsAsync(), null, "Ошибка загрузки статистики");
        SearchCommand = CreateCommand<string>(async (searchText) => await SearchCurriculaAsync(searchText), null, "Ошибка поиска учебных планов");
        ApplyFiltersCommand = CreateCommand(async () => await ApplyFiltersAsync(), null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(async () => await ClearFiltersAsync(), null, "Ошибка очистки фильтров");
        GoToPageCommand = CreateCommand<int>(async (page) => await GoToPageAsync(page), null, "Ошибка навигации по страницам");

        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);

        NextPageCommand = CreateCommand(async () => await NextPageAsync(), canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(async () => await PreviousPageAsync(), canGoPrevious, "Ошибка перехода на предыдущую страницу");

        var hasSelection = this.WhenAnyValue(x => x.SelectedCurriculaCount).Select(count => count > 0);
        BulkActivateCommand = CreateCommand(async () => await BulkSetActiveAsync(true), hasSelection, "Ошибка массовой активации учебных планов");
        BulkDeactivateCommand = CreateCommand(async () => await BulkSetActiveAsync(false), hasSelection, "Ошибка массовой деактивации учебных планов");
        BulkDeleteCommand = CreateCommand(async () => await BulkDeleteAsync(), hasSelection, "Ошибка массового удаления учебных планов");
    }

    private async Task BulkSetActiveAsync(bool activate)
    {
        var selected = Curricula.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0) return;

        foreach (var item in selected)
        {
            if (activate)
            {
                await _curriculumService.ActivateAsync(item.Uid);
                item.IsActive = true;
            }
            else
            {
                await _curriculumService.DeactivateAsync(item.Uid);
                item.IsActive = false;
            }
        }

        ShowSuccess(activate
            ? $"Активировано учебных планов: {selected.Count}"
            : $"Деактивировано учебных планов: {selected.Count}");
    }

    private async Task BulkDeleteAsync()
    {
        var selected = Curricula.Where(c => c.IsSelected).ToList();
        if (selected.Count == 0) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Подтверждение удаления",
            $"Вы уверены, что хотите удалить выбранные учебные планы ({selected.Count})?");
        if (!confirmed) return;

        foreach (var item in selected)
        {
            await _curriculumService.DeleteAsync(item.Uid);
            Curricula.Remove(item);
        }

        ShowSuccess($"Удалено учебных планов: {selected.Count}");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Применение фильтров при изменении
        this.WhenAnyValue(x => x.SelectedDepartmentFilter, x => x.IsActiveFilter, x => x.MinCreditsFilter,
                         x => x.MaxCreditsFilter, x => x.AcademicYearFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ApplyFiltersCommand.Execute().Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Уведомления об изменении computed properties
        this.WhenAnyValue(x => x.SelectedCurriculum)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedCurriculum)))
            .DisposeWith(Disposables);
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        LogInfo("CurriculumViewModel first time loaded");
        await LoadFiltersDataAsync();
        await LoadCurriculaAsync();
        await LoadStatisticsAsync();
    }

    private async Task LoadFiltersDataAsync()
    {
        try
        {
            LogInfo("Loading filters data for curricula");

            var departments = await _departmentService.GetAllAsync();

            Departments.Clear();
            foreach (var department in departments)
            {
                Departments.Add(new DepartmentViewModel(department));
            }

            LogInfo($"Loaded {Departments.Count} departments for filters");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load filters data");
            ShowWarning("Не удалось загрузить данные для фильтров");
        }
    }

    private async Task LoadCurriculaAsync()
    {
        LogInfo($"Loading curricula with filters: SearchText={SearchText}");

        IsLoading = true;
        ShowInfo("Загрузка учебных планов...");

        try
        {
            var (curricula, totalCount) = await _curriculumService.GetPagedAsync(
                CurrentPage,
                PageSize,
                SearchText,
                SelectedDepartmentFilter?.Uid,
                IsActiveFilter,
                MinCreditsFilter,
                MaxCreditsFilter,
                AcademicYearFilter);

            Curricula.Clear();
            foreach (var curriculum in curricula)
            {
                var itemViewModel = new CurriculumItemViewModel(curriculum);
                itemViewModel.WhenAnyValue(x => x.IsSelected)
                    .Subscribe(_ => SelectedCurriculaCount = Curricula.Count(c => c.IsSelected))
                    .DisposeWith(Disposables);
                Curricula.Add(itemViewModel);
            }
            SelectedCurriculaCount = 0;

            TotalCurricula = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            LogInfo($"Loaded {CurriculumCount} curricula, total: {TotalCount}");
            ShowSuccess($"Загружено {Curricula.Count} учебных планов");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load curricula");
            ShowError("Не удалось загрузить список учебных планов");
            Curricula.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing curricula data");
        IsRefreshing = true;

        await LoadFiltersDataAsync();
        await LoadCurriculaAsync();
        await LoadStatisticsAsync();
        ShowSuccess("Данные обновлены");

        IsRefreshing = false;
    }

    /// <summary>
    /// Создает новый учебный план
    /// </summary>
    private async Task CreateCurriculumAsync()
    {
        try
        {
            var result = await _dialogService.ShowCurriculumEditDialogAsync(new Curriculum());
            if (result != null && result is Curriculum curriculum)
            {
                await _curriculumService.CreateAsync(curriculum);
                await LoadCurriculaAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                // await _notificationService.CreateNotificationAsync(
                //     currentPerson.Uid,
                //     "Учебный план создан",
                //     $"Создан новый учебный план: {curriculum.Name}",
                //     NotificationType.Info,
                //     NotificationPriority.Normal);

                await _dialogService.ShowInfoAsync("Успех", "Учебный план успешно создан");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("уже существует"))
        {
            LogError(ex, "Дублирование при создании учебного плана");
            ErrorMessage = $"Учебный план с таким названием уже существует: {ex.Message}";
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при создании учебного плана");
        }
    }

    /// <summary>
    /// Редактирует выбранный учебный план
    /// </summary>
    private async Task EditCurriculumAsync(CurriculumItemViewModel curriculumItemViewModel)
    {
        if (curriculumItemViewModel == null) return;

        try
        {
            var curriculumToEdit = await _curriculumService.GetByUidAsync(curriculumItemViewModel.Uid);
            if (curriculumToEdit == null) return;

            var result = await _dialogService.ShowCurriculumEditDialogAsync(curriculumToEdit);
            if (result != null && result is Curriculum updatedCurriculum)
            {
                await _curriculumService.UpdateAsync(updatedCurriculum);
                await LoadCurriculaAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Учебный план обновлен",
                    $"Обновлен учебный план: {updatedCurriculum.Name}",
                    NotificationType.Info,
                    NotificationPriority.Normal);

                await _dialogService.ShowInfoAsync("Успех", "Учебный план успешно обновлен");
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("конфликт") || ex.Message.Contains("concurrency"))
        {
            LogError(ex, "Конфликт параллельного доступа");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при обновлении учебного плана");
        }
    }

    /// <summary>
    /// Удаляет выбранный учебный план
    /// </summary>
    private async Task DeleteCurriculumAsync(CurriculumItemViewModel curriculumItemViewModel)
    {
        if (curriculumItemViewModel == null) return;

        try
        {
            var curriculumSubjects = await _curriculumService.GetCurriculumSubjectsAsync(new[] { curriculumItemViewModel.Uid });
            
            if (curriculumSubjects.Any())
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Удаление учебного плана",
                    $"Учебный план '{curriculumItemViewModel.Name}' содержит предметы. Удалить вместе с предметами?");

                if (result == false) return;
            }

            var success = await _curriculumService.DeleteAsync(curriculumItemViewModel.Uid);
            if (success)
            {
                await LoadCurriculaAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Учебный план удален",
                    $"Удален учебный план: {curriculumItemViewModel.Name}",
                    NotificationType.Info,
                    NotificationPriority.Normal);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("foreign key") || ex.Message.Contains("связанных данных"))
        {
            LogError(ex, "Ошибка связанных данных");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при удалении учебного плана");
        }
    }

    /// <summary>
    /// Проверяет дублирование учебного плана
    /// </summary>
    private async Task<bool> CheckDuplicateAsync(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;

        try
        {
            var existingCurriculum = await _curriculumService.GetByCodeAsync(code);
            return existingCurriculum != null;
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при проверке дублирования");
            return false;
        }
    }

    /// <summary>
    /// Валидация данных учебного плана
    /// </summary>
    private async Task<DomainValidationResult> ValidateCurriculumAsync(Curriculum curriculum)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(curriculum.Name))
            errors.Add("Название учебного плана обязательно");
        else if (curriculum.Name.Length > 200)
            errors.Add("Название учебного плана не должно превышать 200 символов");

        if (string.IsNullOrWhiteSpace(curriculum.Code))
            errors.Add("Код учебного плана обязателен");
        else if (curriculum.Code.Length > 20)
            errors.Add("Код учебного плана не должен превышать 20 символов");

        // Проверка уникальности кода
        var existingCurriculum = await _curriculumService.GetByCodeAsync(curriculum.Code);
        if (existingCurriculum != null && existingCurriculum.Uid != curriculum.Uid)
            errors.Add($"Учебный план с кодом '{curriculum.Code}' уже существует");

        // Проверка кредитов
        if (curriculum.TotalCredits <= 0)
            errors.Add("Общее количество кредитов должно быть больше 0");
        else if (curriculum.TotalCredits > 300)
            warnings.Add("Рекомендуется не превышать 300 кредитов для учебного плана");

        // Проверка продолжительности
        if (curriculum.DurationSemesters <= 0)
            errors.Add("Продолжительность в семестрах должна быть больше 0");
        else if (curriculum.DurationSemesters > 12)
            warnings.Add("Рекомендуется не превышать 12 семестров");

        // Проверка академического года
        if (curriculum.AcademicYear < 2000 || curriculum.AcademicYear > DateTime.Now.Year + 5)
            warnings.Add("Проверьте корректность академического года");

        // Проверка департамента
        if (curriculum.DepartmentUid != null)
        {
            var department = await _departmentService.GetByUidAsync(curriculum.DepartmentUid.Value);
            if (department == null)
                errors.Add("Указанный департамент не существует");
        }

        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    /// <summary>
    /// Проверка связанных данных перед удалением
    /// </summary>
    private async Task<CurriculumRelatedDataInfo> CheckRelatedDataAsync(Guid curriculumUid)
    {
        var relatedData = new CurriculumRelatedDataInfo();

        try
        {
            // Проверка студентов
            var studentsCount = await _curriculumService.GetStudentsCountAsync(curriculumUid);
            if (studentsCount > 0)
            {
                relatedData.HasStudents = true;
                relatedData.StudentsCount = studentsCount;
                relatedData.RelatedDataDescriptions.Add($"• {studentsCount} студентов используют этот учебный план");
            }

            // Проверка предметов в учебном плане
            var subjectsCount = await _curriculumService.GetSubjectsCountAsync(curriculumUid);
            if (subjectsCount > 0)
            {
                relatedData.HasSubjects = true;
                relatedData.SubjectsCount = subjectsCount;
                relatedData.RelatedDataDescriptions.Add($"• {subjectsCount} предметов включены в учебный план");
            }

            relatedData.HasRelatedData = relatedData.HasStudents || relatedData.HasSubjects;
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to check related data for curriculum {CurriculumUid}");
        }

        return relatedData;
    }

    /// <summary>
    /// Проверка прав доступа
    /// </summary>
    private async Task<bool> HasPermissionAsync(string permission)
    {
        try
        {
            return await _permissionService.HasPermissionAsync(permission);
        }
        catch (Exception ex)
        {
            LogError(ex, $"Failed to check permission: {permission}");
            return false;
        }
    }

    // Остальные методы команд...
    private async Task ViewCurriculumDetailsAsync(CurriculumItemViewModel curriculumViewModel)
    {
        if (curriculumViewModel == null) return;
        LogInfo($"Viewing curriculum details: {CurriculumName}");

        try
        {
            await NavigateToAsync($"curriculum-details/{curriculumViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to curriculum details");
            ShowError("Не удалось открыть детали учебного плана");
        }
    }

    private async Task CopyCurriculumAsync(CurriculumItemViewModel curriculumViewModel)
    {
        if (curriculumViewModel == null) return;
        LogInfo($"Copying curriculum: {CurriculumName}");

        try
        {
            var newName = $"{SelectedCurriculum.Name} (копия)";
            var duplicatedCurriculum = await _curriculumService.DuplicateCurriculumAsync(SelectedCurriculum.Uid, newName);
            if (duplicatedCurriculum != null)
                {
                    await LoadCurriculaAsync();
                ShowSuccess($"Учебный план '{duplicatedCurriculum.Name}' продублирован");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to copy curriculum");
            ShowError("Не удалось скопировать учебный план");
        }
    }

    private async Task ActivateCurriculumAsync(CurriculumItemViewModel curriculumViewModel)
    {
        if (curriculumViewModel == null) return;
        LogInfo($"Activating curriculum: {CurriculumName}");

        try
        {
            await _curriculumService.ActivateAsync(curriculumViewModel.Uid);
            curriculumViewModel.IsActive = true;
            ShowSuccess($"Учебный план '{curriculumViewModel.Name}' активирован");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to activate curriculum");
            ShowError("Не удалось активировать учебный план");
        }
    }

    private async Task ToggleActivationAsync(CurriculumItemViewModel curriculumViewModel)
    {
        if (curriculumViewModel == null) return;

        if (curriculumViewModel.IsActive)
        {
            await DeactivateCurriculumAsync(curriculumViewModel);
        }
        else
        {
            await ActivateCurriculumAsync(curriculumViewModel);
        }
    }

    private async Task DeactivateCurriculumAsync(CurriculumItemViewModel curriculumViewModel)
    {
        if (curriculumViewModel == null) return;
        LogInfo($"Deactivating curriculum: {CurriculumName}");

        try
        {
            await _curriculumService.DeactivateAsync(curriculumViewModel.Uid);
            curriculumViewModel.IsActive = false;
            ShowSuccess($"Учебный план '{curriculumViewModel.Name}' деактивирован");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to deactivate curriculum");
            ShowError("Не удалось деактивировать учебный план");
        }
    }

    private async Task ExportCurriculumAsync(CurriculumItemViewModel curriculumViewModel)
    {
        if (curriculumViewModel == null) return;
        LogInfo($"Exporting curriculum: {CurriculumName}");

        try
        {
            var curriculumUids = new List<Guid> { curriculumViewModel.Uid };
            var result = await _curriculumService.ExportAsync(curriculumUids.First());
            ShowSuccess($"Учебный план '{curriculumViewModel.Name}' экспортирован");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to export curriculum");
            ShowError("Не удалось экспортировать учебный план");
        }
    }

    private async Task ImportCurriculumAsync()
    {
        LogInfo("Importing curriculum");

        try
        {
            // В реальном приложении здесь будет диалог выбора файла
            var importedCurriculum = await _curriculumService.ImportAsync();
            if (importedCurriculum != null)
            {
                Curricula.Add(new CurriculumItemViewModel());
                ShowSuccess($"Учебный план '{importedCurriculum}' импортирован");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to import curriculum");
            ShowError("Не удалось импортировать учебный план");
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            LogInfo($"Loading statistics for curriculum: {SelectedCurriculum?.Name}");
            
            // TODO: Статистика учебного плана будет реализована позже
            // когда будет доступен метод GetCurriculumStatisticsAsync в ICurriculumService
            // var statistics = await _curriculumService.GetCurriculumStatisticsAsync(SelectedCurriculum.Uid);
            
            await _dialogService.ShowMessageAsync(
                "Статистика учебного плана",
                $"Статистика для учебного плана '{SelectedCurriculum?.Name}' временно недоступна.\n" +
                "Функция будет реализована в следующих версиях.");
                
            LogInfo($"Statistics placeholder shown for curriculum: {SelectedCurriculum?.Name}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load curriculum statistics");
            // Statistics = null;
        }
    }

    private async Task SearchCurriculaAsync(string searchText)
    {
        LogInfo($"Searching curricula with text: {SearchText}");
        CurrentPage = 1;
        await LoadCurriculaAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        LogInfo("Applying filters to curricula");
        CurrentPage = 1;
        await LoadCurriculaAsync();
    }

    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing all filters");
        SelectedDepartmentFilter = null;
        IsActiveFilter = null;
        MinCreditsFilter = null;
        MaxCreditsFilter = null;
        AcademicYearFilter = null;
        CurrentPage = 1;
        await LoadCurriculaAsync();
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;

        CurrentPage = page;
        await LoadCurriculaAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadCurriculaAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadCurriculaAsync();
        }
    }

    /// <summary>
    /// Информация о связанных данных учебного плана
    /// </summary>
    public class CurriculumRelatedDataInfo
    {
        public bool HasRelatedData { get; set; }
        public bool HasStudents { get; set; }
        public int StudentsCount { get; set; }
        public bool HasSubjects { get; set; }
        public int SubjectsCount { get; set; }
        public List<string> RelatedDataDescriptions { get; set; } = new();
    }

    /// <summary>
    /// ViewModel для учебного плана
    /// </summary>
    public class CurriculumItemViewModel : ReactiveObject
    {
        [Reactive] public Guid Uid { get; set; }
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public string Code { get; set; } = string.Empty;
        [Reactive] public string? Description { get; set; }
        [Reactive] public int TotalCredits { get; set; }
        [Reactive] public int DurationSemesters { get; set; }
        [Reactive] public int AcademicYear { get; set; }
        [Reactive] public bool IsActive { get; set; }
        [Reactive] public Guid? DepartmentUid { get; set; }
        [Reactive] public DateTime? LastModifiedAt { get; set; }

        // Дополнительные свойства для UI
        [Reactive] public string DepartmentName { get; set; } = string.Empty;
        [Reactive] public int StudentsCount { get; set; }
        [Reactive] public int SubjectsCount { get; set; }
        [Reactive] public bool IsSelected { get; set; }

        public CurriculumItemViewModel() { }

        public CurriculumItemViewModel(Curriculum curriculum)
        {
            Uid = curriculum.Uid;
            Name = curriculum.Name;
            Code = curriculum.Code ?? string.Empty;
            Description = curriculum.Description;
            TotalCredits = curriculum.TotalCredits;
            DurationSemesters = curriculum.DurationSemesters;
            AcademicYear = curriculum.AcademicYear;
            IsActive = curriculum.IsActive;
            DepartmentUid = curriculum.DepartmentUid;
            LastModifiedAt = curriculum.LastModifiedAt;

            // Дополнительные данные из связанных сущностей
            DepartmentName = curriculum.Department?.Name ?? string.Empty;
            StudentsCount = curriculum.Students?.Count ?? 0;
            SubjectsCount = curriculum.CurriculumSubjects?.Count ?? 0;
        }
    }
}

