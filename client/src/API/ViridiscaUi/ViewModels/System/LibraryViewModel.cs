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
using ViridiscaUi.Domain.Entities.System.Enums;
using Microsoft.EntityFrameworkCore; 
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Domain.Services.Auth;
using ViridiscaUi.Domain.Services.Statistic;
using ViridiscaUi.Domain.Entities.Auth;
using ViridiscaUi.Domain.Entities.Library.Enums;
using ViridiscaUi.Domain.Entities.Library;
using ViridiscaUi.Navigations;
using LibraryResourceType = ViridiscaUi.Domain.Entities.Library.Enums.ResourceType;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для управления библиотечной системой
/// Следует принципам SOLID и чистой архитектуры
/// </summary>
[Route("library", 
    DisplayName = "Библиотека", 
    IconKey = "LibraryShelves", 
    Order = 1,
    Group = "Система",
    ShowInMenu = true,
    Description = "Управление библиотечными ресурсами и займами")]
public class LibraryViewModel : RoutableViewModelBase
{
    private readonly ILibraryService _libraryService;
    private readonly IPersonService _personService; 
    private readonly IStatusService _statusService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    // === СВОЙСТВА ===
    
    [Reactive] public ObservableCollection<LibraryResourceViewModel> Resources { get; set; } = new();
    [Reactive] public ObservableCollection<LibraryLoanViewModel> Loans { get; set; } = new();
    [Reactive] public LibraryResourceViewModel? SelectedResource { get; set; }
    [Reactive] public LibraryLoanViewModel? SelectedLoan { get; set; }
    [Reactive] public string SearchText { get; set; } = string.Empty;
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public bool IsRefreshing { get; set; }
    [Reactive] public LibraryStatistics? Statistics { get; set; }
    [Reactive] public LibraryViewMode ViewMode { get; set; } = LibraryViewMode.Resources;
    
    // Statistics properties
    [Reactive] public int TotalResources { get; set; }
    [Reactive] public int AvailableResources { get; set; }
    [Reactive] public int BorrowedResources { get; set; }
    [Reactive] public int OverdueResources { get; set; }
    [Reactive] public int TotalLoans { get; set; }
    [Reactive] public int ActiveLoans { get; set; }
    [Reactive] public int OverdueLoans { get; set; }
    [Reactive] public int BorrowerCount { get; set; }
    
    // Фильтры
    [Reactive] public string? ResourceTypeFilter { get; set; }
    [Reactive] public string? StatusFilter { get; set; }
    [Reactive] public string? CategoryFilter { get; set; }
    
    // Фильтры для ресурсов
    [Reactive] public string ResourceSearchText { get; set; } = string.Empty;
    [Reactive] public bool? AvailableFilter { get; set; }
    [Reactive] public string? AuthorFilter { get; set; }
    [Reactive] public string? PublisherFilter { get; set; }
    [Reactive] public int? YearFromFilter { get; set; }
    [Reactive] public int? YearToFilter { get; set; }
    
    // Фильтры для займов
    [Reactive] public string LoanSearchText { get; set; } = string.Empty;
    [Reactive] public bool? ActiveLoansFilter { get; set; }
    [Reactive] public bool? OverdueLoansFilter { get; set; }
    
    // Дополнительные фильтры для займов
    [Reactive] public ObservableCollection<PersonViewModel> Persons { get; set; } = new();
    [Reactive] public PersonViewModel? SelectedPersonFilter { get; set; }
    [Reactive] public bool? OverdueFilter { get; set; }
    [Reactive] public DateTime? LoanDateFromFilter { get; set; }
    [Reactive] public DateTime? LoanDateToFilter { get; set; }
    [Reactive] public DateTime? DueDateFromFilter { get; set; }
    [Reactive] public DateTime? DueDateToFilter { get; set; }
    
    // Коллекция заемщиков
    [Reactive] public ObservableCollection<LibraryBorrowerViewModel> Borrowers { get; set; } = new();
    
    // Пагинация
    [Reactive] public int CurrentPage { get; set; } = 1;
    [Reactive] public int PageSize { get; set; } = 15;
    [Reactive] public int TotalPages { get; set; }
    [Reactive] public int TotalResourcesCount { get; set; }
    [Reactive] public int TotalLoansCount { get; set; }

    // Computed properties
    public bool HasSelectedResource => SelectedResource != null;
    public bool HasSelectedLoan => SelectedLoan != null;
    public bool HasStatistics => Statistics != null;
    public bool IsResourcesView => ViewMode == LibraryViewMode.Resources;
    public bool IsLoansView => ViewMode == LibraryViewMode.Loans;

    // Computed properties for UI binding
    public string ResourceTitle => SelectedResource?.Title ?? "Неизвестный ресурс";
    public Guid ResourceUid => SelectedResource?.Uid ?? Guid.Empty;
    public Guid LoanUid => SelectedLoan?.Uid ?? Guid.Empty;

    // === КОМАНДЫ ===
    
    public ReactiveCommand<Unit, Unit> LoadDataCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> RefreshCommand { get; private set; } = null!;
    
    // Команды для ресурсов
    public ReactiveCommand<Unit, Unit> CreateResourceCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryResourceViewModel, Unit> EditResourceCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryResourceViewModel, Unit> DeleteResourceCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryResourceViewModel, Unit> ViewResourceDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryResourceViewModel, Unit> LoanResourceCommand { get; private set; } = null!;
    
    // Команды для займов
    public ReactiveCommand<LibraryLoanViewModel, Unit> ReturnLoanCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryLoanViewModel, Unit> ExtendLoanCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryLoanViewModel, Unit> ViewLoanDetailsCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> CheckOverdueLoansCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> SendOverdueNotificationsCommand { get; private set; } = null!;
    
    // Общие команды
    public ReactiveCommand<Unit, Unit> LoadStatisticsCommand { get; private set; } = null!;
    public ReactiveCommand<string, Unit> SearchCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ApplyFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ClearFiltersCommand { get; private set; } = null!;
    public ReactiveCommand<LibraryViewMode, Unit> ChangeViewModeCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ExportDataCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> ImportResourcesCommand { get; private set; } = null!;
    public ReactiveCommand<int, Unit> GoToPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;

    public LibraryViewModel(
        IScreen hostScreen,
        ILibraryService libraryService,
        IPersonService personService, 
        IStatusService statusService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IAuthService authService,
        IDialogService dialogService) : base(hostScreen)
    {
        _libraryService = libraryService ?? throw new ArgumentNullException(nameof(libraryService));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService)); 
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
        LoadDataCommand = CreateCommand(LoadDataAsync, null, "Ошибка загрузки данных библиотеки");
        RefreshCommand = CreateCommand(RefreshAsync, null, "Ошибка обновления данных");
        
        // Команды для ресурсов
        CreateResourceCommand = CreateCommand(CreateResourceAsync, null, "Ошибка создания ресурса");
        EditResourceCommand = CreateCommand<LibraryResourceViewModel>(EditResourceAsync, null, "Ошибка редактирования ресурса");
        DeleteResourceCommand = CreateCommand<LibraryResourceViewModel>(DeleteResourceAsync, null, "Ошибка удаления ресурса");
        ViewResourceDetailsCommand = CreateCommand<LibraryResourceViewModel>(ViewResourceDetailsAsync, null, "Ошибка просмотра деталей ресурса");
        LoanResourceCommand = CreateCommand<LibraryResourceViewModel>(LoanResourceAsync, null, "Ошибка выдачи ресурса");
        
        // Команды для займов
        ReturnLoanCommand = CreateCommand<LibraryLoanViewModel>(ReturnLoanAsync, null, "Ошибка возврата ресурса");
        ExtendLoanCommand = CreateCommand<LibraryLoanViewModel>(ExtendLoanAsync, null, "Ошибка продления займа");
        ViewLoanDetailsCommand = CreateCommand<LibraryLoanViewModel>(ViewLoanDetailsAsync, null, "Ошибка просмотра деталей займа");
        CheckOverdueLoansCommand = CreateCommand(CheckOverdueLoansAsync, null, "Ошибка проверки просроченных займов");
        SendOverdueNotificationsCommand = CreateCommand(SendOverdueNotificationsAsync, null, "Ошибка отправки уведомлений");
        
        // Общие команды
        LoadStatisticsCommand = CreateCommand(LoadStatisticsAsync, null, "Ошибка загрузки статистики");
        SearchCommand = CreateCommand<string>(SearchAsync, null, "Ошибка поиска");
        ApplyFiltersCommand = CreateCommand(ApplyFiltersAsync, null, "Ошибка применения фильтров");
        ClearFiltersCommand = CreateCommand(ClearFiltersAsync, null, "Ошибка очистки фильтров");
        ChangeViewModeCommand = CreateCommand<LibraryViewMode>(ChangeViewModeAsync, null, "Ошибка смены режима просмотра");
        ExportDataCommand = CreateCommand(ExportDataAsync, null, "Ошибка экспорта данных");
        ImportResourcesCommand = CreateCommand(ImportResourcesAsync, null, "Ошибка импорта ресурсов");
        GoToPageCommand = CreateCommand<int>(GoToPageAsync, null, "Ошибка навигации по страницам");
        
        var canGoNext = this.WhenAnyValue(x => x.CurrentPage, x => x.TotalPages, (current, total) => current < total);
        var canGoPrevious = this.WhenAnyValue(x => x.CurrentPage, current => current > 1);
        
        NextPageCommand = CreateCommand(NextPageAsync, canGoNext, "Ошибка перехода на следующую страницу");
        PreviousPageCommand = CreateCommand(PreviousPageAsync, canGoPrevious, "Ошибка перехода на предыдущую страницу");
    }

    private void SetupSubscriptions()
    {
        // Автопоиск при изменении текста поиска
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(500))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(searchText => SearchCommand.Execute(searchText ?? string.Empty).Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Автоматическое применение фильтров
        this.WhenAnyValue(x => x.ResourceTypeFilter, x => x.AvailableFilter)
            .Throttle(TimeSpan.FromMilliseconds(300))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ApplyFiltersCommand.Execute().Subscribe(_ => { }, _ => { }))
            .DisposeWith(Disposables);

        // Обновление computed properties
        this.WhenAnyValue(x => x.SelectedResource)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedResource)))
            .DisposeWith(Disposables);

        this.WhenAnyValue(x => x.SelectedLoan)
            .Subscribe(_ => this.RaisePropertyChanged(nameof(HasSelectedLoan)))
            .DisposeWith(Disposables);
    }

    protected override async Task OnFirstTimeLoadedAsync()
    {
        LogInfo("LibraryViewModel first time loaded");
        await LoadFiltersDataAsync();
        await LoadDataAsync();
        await LoadStatisticsAsync();
    }

    private async Task LoadFiltersDataAsync()
    {
        try
        {
            LogInfo("Loading filters data");
            
            var (people, _) = await _personService.GetPagedAsync(1, 1000, string.Empty);
            
            Borrowers.Clear();
            foreach (var person in people)
            {
                Borrowers.Add(new LibraryBorrowerViewModel(person));
            }
            
            LogInfo($"Loaded {BorrowerCount} borrowers for filters");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load filters data");
            ShowWarning("Не удалось загрузить данные для фильтров");
        }
    }

    private async Task LoadDataAsync()
    {
        LogInfo($"Loading library data with ViewMode={ViewMode}");
        
        IsLoading = true;
        ShowInfo($"Загрузка {(IsResourcesView ? "ресурсов" : "займов")}...");

        try
        {
            if (IsResourcesView)
            {
                await LoadResourcesAsync();
            }
            else
            {
                await LoadLoansAsync();
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load library data");
            ShowError($"Не удалось загрузить {(IsResourcesView ? "ресурсы" : "займы")}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadResourcesAsync()
    {
        try
        {
            IsLoading = true;

            var (resources, totalCount) = await _libraryService.GetResourcesPagedAsync(
                CurrentPage,
                PageSize,
                ResourceSearchText);

            TotalResources = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            Resources.Clear();
            foreach (var resource in resources)
            {
                var resourceViewModel = new LibraryResourceViewModel
                {
                    Uid = resource.Uid,
                    Title = resource.Title,
                    Author = resource.Author,
                    Publisher = resource.Publisher,
                    PublicationYear = resource.PublishedDate?.Year,
                    ISBN = resource.ISBN,
                    ResourceType = resource.ResourceType,
                    Description = resource.Description,
                    Location = resource.Location,
                    TotalCopies = resource.TotalCopies,
                    AvailableCopies = resource.AvailableCopies,
                    LastModifiedAt = resource.LastModifiedAt
                };
                Resources.Add(resourceViewModel);
            }

            LogInfo($"Resources loaded successfully: {Resources.Count}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load resources");
            ShowError("Не удалось загрузить ресурсы");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadLoansAsync()
    {
        try
        {
            IsLoading = true;

            var (loans, totalCount) = await _libraryService.GetLoansPagedAsync(
                CurrentPage,
                PageSize,
                LoanSearchText);

            TotalLoans = totalCount;
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize);

            Loans.Clear();
            foreach (var loan in loans)
            {
                var loanViewModel = new LibraryLoanViewModel
                {
                    Uid = loan.Uid,
                    ResourceUid = loan.ResourceUid,
                    PersonUid = loan.PersonUid,
                    LoanedAt = loan.LoanedAt,
                    DueDate = loan.DueDate,
                    ReturnedAt = loan.ReturnedAt,
                    ResourceTitle = loan.Resource?.Title ?? "Неизвестный ресурс",
                    BorrowerName = $"{loan.Person?.FirstName} {loan.Person?.LastName}".Trim()
                };
                Loans.Add(loanViewModel);
            }

            LogInfo($"Loans loaded successfully: {Loans.Count}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load loans");
            ShowError("Не удалось загрузить займы");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshAsync()
    {
        LogInfo("Refreshing library data");
        IsRefreshing = true;
        
        await LoadFiltersDataAsync();
        await LoadDataAsync();
        await LoadStatisticsAsync();
        ShowSuccess("Данные обновлены");
        
        IsRefreshing = false;
    }

    /// <summary>
    /// Создает новый ресурс
    /// </summary>
    private async Task CreateResourceAsync()
    {
        try
        {
            var result = await _dialogService.ShowLibraryResourceEditDialogAsync(new LibraryResource());
            if (result != null && result is LibraryResource resource)
            {
                await _libraryService.CreateAsync(resource);
                await LoadResourcesAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Ресурс создан",
                    $"Библиотечный ресурс '{resource.Title}' успешно создан",
                    NotificationType.Success,
                    Domain.Entities.System.Enums.NotificationPriority.Normal);
            }
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Validation failed for library resource creation");
            ShowError($"Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            ShowError($"Ошибка при создании ресурса: {ex.Message}");
            LogError(ex, "Ошибка при создании ресурса");
        }
    }

    /// <summary>
    /// Редактирование ресурса с optimistic locking
    /// </summary>
    private async Task EditResourceAsync(LibraryResourceViewModel resourceViewModel)
    {
        if (resourceViewModel == null) return;

        LogInfo($"Editing library resource: {ResourceTitle}");

        // Проверка прав доступа
        if (!await HasPermissionAsync("Library.Update"))
        {
            ShowError("У вас нет прав для редактирования библиотечных ресурсов");
            return;
        }

        try
        {
            // Получение актуальных данных с проверкой версии
            var resource = await _libraryService.GetResourceByUidAsync(resourceViewModel.Uid);
            if (resource == null)
            {
                ShowError("Ресурс не найден");
                await LoadDataAsync(); // Обновляем список
                return;
            }

            // Проверка optimistic locking
            if (resource.LastModifiedAt != resourceViewModel.LastModifiedAt)
            {
                var result = await _dialogService.ShowConfirmationAsync(
                    "Конфликт версий",
                    "Ресурс был изменен другим пользователем. Хотите перезаписать изменения?");
                
                if (result == false)
                {
                    await LoadDataAsync(); // Обновляем список
                    return;
                }
            }

            var dialogResult = await _dialogService.ShowLibraryResourceEditDialogAsync(resource);
            if (dialogResult == null)
            {
                LogDebug("Library resource editing cancelled by user");
                return;
            }

            // Валидация изменений
            var validationResult = await ValidateResourceAsync(dialogResult);
            if (!validationResult.IsValid)
            {
                await _dialogService.ShowValidationErrorsAsync("Ошибки валидации", validationResult.Errors);
                return;
            }

            // Обновление ресурса
            var updatedResource = await _libraryService.UpdateResourceAsync(dialogResult);
            
            // Обновление UI
            if (IsResourcesView)
            {
                var index = Resources.IndexOf(resourceViewModel);
                if (index >= 0)
                {
                    Resources[index] = new LibraryResourceViewModel(updatedResource);
                    if (SelectedResource?.Uid == updatedResource.Uid)
                    {
                        SelectedResource = Resources[index];
                    }
                }
            }
            
            // Автоматическое обновление статистики
            await LoadStatisticsAsync();
            
            LogInfo($"Library resource updated successfully: {ResourceTitle}");
            var currentPerson = await _authService.GetCurrentPersonAsync();
            await _notificationService.CreateNotificationAsync(
                currentPerson.Uid,
                "Ресурс обновлен",
                $"Библиотечный ресурс '{updatedResource.Title}' успешно обновлен",
                NotificationType.Success,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            LogError(ex, "Concurrency conflict while updating library resource");
            ShowError("Ресурс был изменен другим пользователем. Обновите данные и попробуйте снова.");
            await LoadDataAsync();
        }
        catch (ArgumentException ex)
        {
            LogError(ex, "Validation failed for library resource update");
            ShowError($"Ошибка валидации: {ex.Message}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to update library resource");
            ShowError("Не удалось обновить ресурс. Попробуйте еще раз.");
        }
    }

    /// <summary>
    /// Удаляет выбранный ресурс
    /// </summary>
    private async Task DeleteResourceAsync(LibraryResourceViewModel resourceViewModel)
    {
        if (resourceViewModel == null) return;

        try
        {
            var result = await _dialogService.ShowConfirmationAsync(
                "Подтверждение удаления",
                $"Вы уверены, что хотите удалить ресурс '{resourceViewModel.Title}'?");

            if (result)
            {
                await _libraryService.DeleteAsync(resourceViewModel.Uid);
                await LoadResourcesAsync();
                var currentPerson = await _authService.GetCurrentPersonAsync();
                await _notificationService.CreateNotificationAsync(
                    currentPerson.Uid,
                    "Ресурс удален",
                    $"Библиотечный ресурс '{resourceViewModel.Title}' успешно удален",
                    NotificationType.Success,
                    Domain.Entities.System.Enums.NotificationPriority.Normal);
            }
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("foreign key") || ex.Message.Contains("связанных данных"))
        {
            LogError(ex, "Ошибка связанных данных");
        }
        catch (Exception ex)
        {
            LogError(ex, "Ошибка при удалении ресурса");
        }
    }

    /// <summary>
    /// Валидация данных ресурса
    /// </summary>
    private async Task<DomainValidationResult> ValidateResourceAsync(LibraryResource resource)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Базовая валидация
        if (string.IsNullOrWhiteSpace(resource.Title))
            errors.Add("Название ресурса обязательно");
        else if (resource.Title.Length > 500)
            errors.Add("Название ресурса не должно превышать 500 символов");

        if (resource.TotalCopies <= 0)
            errors.Add("Количество экземпляров должно быть больше 0");

        if (resource.AvailableCopies < 0)
            errors.Add("Количество доступных экземпляров не может быть отрицательным");

        if (resource.AvailableCopies > resource.TotalCopies)
            errors.Add("Количество доступных экземпляров не может превышать общее количество");

        // Проверка ISBN если указан
        if (!string.IsNullOrWhiteSpace(resource.ISBN))
        {
            if (resource.ISBN.Length != 10 && resource.ISBN.Length != 13)
                warnings.Add("ISBN должен содержать 10 или 13 символов");
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
    private async Task<LibraryResourceRelatedDataInfo> CheckRelatedDataAsync(Guid resourceUid)
    {
        var info = new LibraryResourceRelatedDataInfo();
        
        try
        {
            // Проверяем активные займы
            var activeLoans = await _libraryService.GetActiveLoansForResourceAsync(resourceUid);
            var activeLoansCount = activeLoans?.Count() ?? 0;
            if (activeLoansCount > 0)
            {
                info.HasActiveLoans = true;
                info.ActiveLoansCount = activeLoansCount;
                info.RelatedDataDescriptions.Add($"• Активные займы: {activeLoansCount}");
            }
            
            // Проверяем историю займов
            var allLoans = await _libraryService.GetLoansForResourceAsync(resourceUid);
            var allLoansCount = allLoans?.Count() ?? 0;
            if (allLoansCount > 0)
            {
                info.HasLoans = true;
                info.LoansCount = allLoansCount;
                info.RelatedDataDescriptions.Add($"• Всего займов в истории: {allLoansCount}");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, $"Error checking related data for resource: {ResourceUid}");
        }
        
        return info;
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
    private async Task ViewResourceDetailsAsync(LibraryResourceViewModel resourceViewModel)
    {
        if (resourceViewModel == null) return;
        LogInfo($"Viewing library resource details: {ResourceTitle}");
        
        try
        {
            await NavigateToAsync($"library-resource-details/{resourceViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to library resource details");
            ShowError("Не удалось открыть детали ресурса");
        }
    }

    private async Task LoanResourceAsync(LibraryResourceViewModel resourceViewModel)
    {
        if (resourceViewModel == null) return;
        LogInfo($"Creating loan for resource: {ResourceTitle}");
        
        try
        {
            if (!resourceViewModel.IsAvailable)
            {
                ShowError("Ресурс недоступен для выдачи");
                return;
            }

            // Создание нового займа через диалог
            var loanDialog = await _dialogService.ShowCreateLoanDialogAsync();
            if (loanDialog != null && loanDialog is LibraryLoan loanData)
            {
                var loan = new LibraryLoan
                {
                    Uid = Guid.NewGuid(),
                    ResourceUid = resourceViewModel.Uid,
                    PersonUid = loanData.PersonUid,
                    LoanedAt = DateTime.Now,
                    DueDate = loanData.DueDate
                };

                var createdLoan = await _libraryService.CreateLoanAsync(loan);
                
                // Обновление доступности ресурса
                resourceViewModel.AvailableCopies--;
                
                ShowSuccess($"Ресурс '{resourceViewModel.Title}' выдан");
                
                // Уведомление
                await _notificationService.CreateNotificationAsync(
                    loan.PersonUid,
                    "Ресурс выдан",
                    $"Вам выдан ресурс '{resourceViewModel.Title}'",
                    NotificationType.Info,
                    Domain.Entities.System.Enums.NotificationPriority.Normal);
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to create loan for resource");
            ShowError("Не удалось выдать ресурс");
        }
    }

    private async Task ReturnLoanAsync(LibraryLoanViewModel loanViewModel)
    {
        if (loanViewModel == null) return;
        LogInfo($"Returning loan: {LoanUid}");
        
        try
        {
            await _libraryService.ReturnLoanAsync(loanViewModel.Uid);
            await LoadLoansAsync();
            
            ShowSuccess($"Займ ресурса '{loanViewModel.ResourceTitle}' успешно возвращен");
            
            // Уведомление
            await _notificationService.CreateNotificationAsync(
                loanViewModel.PersonUid,
                "Ресурс возвращен",
                $"Ресурс '{loanViewModel.ResourceTitle}' успешно возвращен",
                NotificationType.Info,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to return loan");
            ShowError("Не удалось вернуть займ");
        }
    }

    private async Task ExtendLoanAsync(LibraryLoanViewModel loanViewModel)
    {
        if (loanViewModel == null) return;
        LogInfo($"Extending loan: {LoanUid}");
        
        try
        {
            var dialogResult = await _dialogService.ShowExtendLoanDialogAsync(loanViewModel);
            if (dialogResult is DateTime newDueDate)
            {
                await _libraryService.ExtendLoanAsync(loanViewModel.Uid, newDueDate);
                loanViewModel.DueDate = newDueDate;
                
                ShowSuccess("Срок займа продлен");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to extend loan");
            ShowError("Не удалось продлить займ");
        }
    }

    private async Task ViewLoanDetailsAsync(LibraryLoanViewModel loanViewModel)
    {
        if (loanViewModel == null) return;
        LogInfo($"Viewing loan details: {LoanUid}");
        
        try
        {
            await NavigateToAsync($"library-loan-details/{loanViewModel.Uid}");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to navigate to loan details");
            ShowError("Не удалось открыть детали займа");
        }
    }

    private async Task CheckOverdueLoansAsync()
    {
        LogInfo("Checking overdue loans");
        
        try
        {
            var overdueLoans = await _libraryService.GetOverdueLoansAsync();
            if (overdueLoans.Any())
            {
                await _dialogService.ShowOverdueLoansDialogAsync();
            }
            else
            {
                ShowSuccess("Просроченных займов не найдено");
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to check overdue loans");
            ShowError("Не удалось проверить просроченные займы");
        }
    }

    private async Task SendOverdueNotificationsAsync()
    {
        LogInfo("Sending overdue notifications");
        
        try
        {
            await ProcessOverdueLoansAsync();
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to send overdue notifications");
            ShowError("Не удалось отправить уведомления");
        }
    }

    /// <summary>
    /// Обработка просроченных займов
    /// </summary>
    private async Task ProcessOverdueLoansAsync()
    {
        try
        {
            LogInfo("Processing overdue loans");
            IsLoading = true;

            var overdueLoans = await _libraryService.GetOverdueLoansAsync();
            
            foreach (var loan in overdueLoans)
            {
                if (loan is LibraryLoan libraryLoan)
                {
                    // Отправка уведомления заемщику
                    await _notificationService.CreateNotificationAsync(
                        libraryLoan.PersonUid,
                        "Просроченный займ",
                        $"У вас просрочен займ ресурса. Срок возврата: {libraryLoan.DueDate:dd.MM.yyyy}",
                        NotificationType.Warning,
                        Domain.Entities.System.Enums.NotificationPriority.High);
                }
            }

            ShowSuccess($"Обработано {overdueLoans.Count()} просроченных займов");
            LogInfo($"Processed {overdueLoans.Count()} overdue loans");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to process overdue loans");
            ShowError("Не удалось обработать просроченные займы");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            var stats = await GetStatisticsAsync();
            
            TotalResources = stats.TotalResources;
            TotalResourcesCount = stats.TotalResources;
            AvailableResources = stats.AvailableResources;
            BorrowedResources = stats.BorrowedResources;
            OverdueResources = stats.OverdueResources;
            TotalLoans = stats.TotalLoans;
            ActiveLoans = stats.ActiveLoans;
            OverdueLoans = stats.OverdueLoans;
            
            LogInfo("Library statistics loaded successfully");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load library statistics");
            ShowError("Не удалось загрузить статистику библиотеки");
        }
    }

    private async Task<LibraryStatistics> GetStatisticsAsync()
    {
        try
        {
            var totalResources = await _libraryService.GetTotalResourcesCountAsync();
            var availableResources = await _libraryService.GetAvailableResourcesCountAsync();
            var borrowedResources = await _libraryService.GetBorrowedResourcesCountAsync();
            var overdueResources = await _libraryService.GetOverdueResourcesCountAsync();
            var totalLoans = await _libraryService.GetTotalLoansCountAsync();
            var activeLoans = await _libraryService.GetActiveLoansCountAsync();
            var overdueLoans = await _libraryService.GetOverdueLoansCountAsync();

            return new LibraryStatistics
            {
                TotalResources = totalResources,
                AvailableResources = availableResources,
                BorrowedResources = borrowedResources,
                OverdueResources = overdueResources,
                TotalLoans = totalLoans,
                ActiveLoans = activeLoans,
                OverdueLoans = overdueLoans
            };
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to get library statistics");
            throw;
        }
    }

    // Helper class for statistics
    public class LibraryStatistics
    {
        public int TotalResources { get; set; }
        public int AvailableResources { get; set; }
        public int BorrowedResources { get; set; }
        public int OverdueResources { get; set; }
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
    }

    private async Task SearchAsync(string searchText)
    {
        LogInfo($"Searching library with text: {SearchText}");
        CurrentPage = 1;
        await LoadDataAsync();
    }

    private async Task ApplyFiltersAsync()
    {
        LogInfo("Applying filters to library");
        CurrentPage = 1;
        await LoadDataAsync();
    }

    private async Task ClearFiltersAsync()
    {
        LogInfo("Clearing all filters");
        ResourceTypeFilter = null;
        AvailableFilter = null;
        AuthorFilter = null;
        PublisherFilter = null;
        YearFromFilter = null;
        YearToFilter = null;
        SelectedPersonFilter = null;
        OverdueFilter = null;
        LoanDateFromFilter = null;
        LoanDateToFilter = null;
        DueDateFromFilter = null;
        DueDateToFilter = null;
        CurrentPage = 1;
        await LoadDataAsync();
    }

    private async Task ChangeViewModeAsync(LibraryViewMode viewMode)
    {
        LogInfo($"Changing view mode to: {ViewMode}");
        ViewMode = viewMode;
        CurrentPage = 1;
        // LoadDataAsync будет вызван автоматически через подписку
    }

    private async Task ExportDataAsync()
    {
        LogInfo("Exporting library data");
        
        try
        {
            if (IsResourcesView)
            {
                await ExportResourcesAsync();
            }
            else
            {
                await ExportLoansAsync();
            }
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to export library data");
            ShowError("Не удалось экспортировать данные");
        }
    }

    /// <summary>
    /// Экспорт ресурсов в Excel
    /// </summary>
    private async Task ExportResourcesAsync()
    {
        try
        {
            await _libraryService.ExportResourcesAsync("excel");
            await _notificationService.CreateNotificationAsync(
                Guid.NewGuid(), // PersonUid - заглушка
                "Экспорт завершен",
                "Ресурсы успешно экспортированы",
                NotificationType.Success,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
        catch (Exception ex)
        {
            await _notificationService.CreateNotificationAsync(
                Guid.NewGuid(), // PersonUid - заглушка
                "Ошибка экспорта",
                $"Ошибка при экспорте ресурсов: {ex.Message}",
                NotificationType.Error,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
    }

    /// <summary>
    /// Экспорт займов в Excel
    /// </summary>
    private async Task ExportLoansAsync()
    {
        try
        {
            await _libraryService.ExportLoansAsync("excel");
            await _notificationService.CreateNotificationAsync(
                Guid.NewGuid(), // PersonUid - заглушка
                "Экспорт завершен",
                "Займы успешно экспортированы",
                NotificationType.Success,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
        catch (Exception ex)
        {
            await _notificationService.CreateNotificationAsync(
                Guid.NewGuid(), // PersonUid - заглушка
                "Ошибка экспорта",
                $"Ошибка при экспорте займов: {ex.Message}",
                NotificationType.Error,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
    }

    /// <summary>
    /// Импорт ресурсов из файла
    /// </summary>
    private async Task ImportResourcesAsync()
    {
        try
        {
            // TODO: Показать диалог выбора файла
            var filePath = "sample.xlsx"; // Заглушка
            var count = await _libraryService.ImportResourcesAsync(filePath);
            await LoadResourcesAsync();
            await _notificationService.CreateNotificationAsync(
                Guid.NewGuid(), // PersonUid - заглушка
                "Импорт завершен",
                $"Импортировано {count} ресурсов",
                NotificationType.Success,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
        catch (Exception ex)
        {
            await _notificationService.CreateNotificationAsync(
                Guid.NewGuid(), // PersonUid - заглушка
                "Ошибка импорта",
                $"Ошибка при импорте ресурсов: {ex.Message}",
                NotificationType.Error,
                Domain.Entities.System.Enums.NotificationPriority.Normal);
        }
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > TotalPages) return;
        
        CurrentPage = page;
        await LoadDataAsync();
    }

    private async Task NextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadDataAsync();
        }
    }

    private async Task PreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadDataAsync();
        }
    }

    private async Task LoadBorrowersAsync()
    {
        try
        {
            // Получаем всех людей из системы
            var people = await _personService.GetAllPersonsAsync();
            
            Borrowers.Clear();
            foreach (var person in people)
            {
                var borrowerViewModel = new LibraryBorrowerViewModel
                {
                    PersonUid = person.Uid,
                    FullName = $"{person.FirstName} {person.LastName}",
                    Email = person.Email ?? string.Empty,
                    Phone = person.Phone ?? string.Empty
                };
                Borrowers.Add(borrowerViewModel);
            }
            
            LogInfo("Borrowers loaded successfully");
        }
        catch (Exception ex)
        {
            LogError(ex, "Failed to load borrowers");
            ShowError("Не удалось загрузить список заемщиков");
        }
    }
}

/// <summary>
/// Режимы просмотра библиотеки
/// </summary>
public enum LibraryViewMode
{
    Resources,
    Loans
}

/// <summary>
/// Информация о связанных данных библиотечного ресурса
/// </summary>
public class LibraryResourceRelatedDataInfo
{
    public bool HasRelatedData { get; set; }
    public bool HasActiveLoans { get; set; }
    public int ActiveLoansCount { get; set; }
    public bool HasLoans { get; set; }
    public int LoansCount { get; set; }
    public List<string> RelatedDataDescriptions { get; set; } = new();
}

/// <summary>
/// ViewModel для библиотечного ресурса
/// </summary>
public class LibraryResourceViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Title { get; set; } = string.Empty;
    [Reactive] public string? Author { get; set; }
    [Reactive] public string? Publisher { get; set; }
    [Reactive] public int? PublicationYear { get; set; }
    [Reactive] public string? ISBN { get; set; }
    [Reactive] public LibraryResourceType ResourceType { get; set; }
    [Reactive] public string? Description { get; set; }
    [Reactive] public string? Location { get; set; }
    [Reactive] public int TotalCopies { get; set; }
    [Reactive] public int AvailableCopies { get; set; }
    [Reactive] public DateTime LastModifiedAt { get; set; }

    // Вычисляемые свойства
    public bool IsAvailable => AvailableCopies > 0;
    public LibraryResourceType Type => ResourceType; // Алиас для совместимости

    public LibraryResourceViewModel() { }

    public LibraryResourceViewModel(LibraryResource resource)
    {
        Uid = resource.Uid;
        Title = resource.Title;
        Author = resource.Author;
        Publisher = resource.Publisher;
        PublicationYear = resource.PublishedDate?.Year;
        ISBN = resource.ISBN;
        ResourceType = resource.ResourceType;
        Description = resource.Description;
        Location = resource.Location;
        TotalCopies = resource.TotalCopies;
        AvailableCopies = resource.AvailableCopies;
        LastModifiedAt = resource.LastModifiedAt;
    }
}

/// <summary>
/// ViewModel для займа библиотечного ресурса
/// </summary>
public class LibraryLoanViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public Guid ResourceUid { get; set; }
    [Reactive] public Guid PersonUid { get; set; }
    [Reactive] public DateTime LoanedAt { get; set; }
    [Reactive] public DateTime DueDate { get; set; }
    [Reactive] public DateTime? ReturnedAt { get; set; }

    // Дополнительные свойства для отображения
    [Reactive] public string ResourceTitle { get; set; } = string.Empty;
    [Reactive] public string BorrowerName { get; set; } = string.Empty;

    public LibraryLoanViewModel() { }

    public LibraryLoanViewModel(LibraryLoan loan)
    {
        Uid = loan.Uid;
        ResourceUid = loan.ResourceUid;
        PersonUid = loan.PersonUid;
        LoanedAt = loan.LoanedAt;
        DueDate = loan.DueDate;
        ReturnedAt = loan.ReturnedAt;
        ResourceTitle = loan.Resource?.Title ?? "Неизвестный ресурс";
        BorrowerName = $"{loan.Person?.FirstName} {loan.Person?.LastName}".Trim();
    }

    // Дополнительные свойства для совместимости
    public DateTime LoanDate
    {
        get => LoanedAt;
        set => LoanedAt = value;
    }

    public DateTime? ReturnDate
    {
        get => ReturnedAt;
        set => ReturnedAt = value;
    }

    public bool IsReturned => ReturnedAt != null;
    public bool IsOverdue => !IsReturned && DateTime.Now > DueDate;
    public decimal? FineAmount { get; set; }
    public string PersonName
    {
        get => BorrowerName;
        set => BorrowerName = value;
    }
}

/// <summary>
/// ViewModel для заемщика библиотечных ресурсов
/// </summary>
public class LibraryBorrowerViewModel : ReactiveObject
{
    [Reactive] public Guid PersonUid { get; set; }
    [Reactive] public string FullName { get; set; } = string.Empty;
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string Phone { get; set; } = string.Empty;
    [Reactive] public int ActiveLoansCount { get; set; }
    [Reactive] public int OverdueLoansCount { get; set; }

    public LibraryBorrowerViewModel() { }

    public LibraryBorrowerViewModel(Person person)
    {
        PersonUid = person.Uid;
        FullName = $"{person.FirstName} {person.LastName}".Trim();
        Email = person.Email ?? string.Empty;
        Phone = person.Phone ?? string.Empty;
    }
} 

