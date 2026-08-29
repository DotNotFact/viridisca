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
using ViridiscaUi.Domain.Services;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Services.Notification;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.Navigations;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual academic period with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
[Route("academic-periods", 
    DisplayName = "Академические периоды", 
    IconKey = "CalendarRange", 
    Order = 9,
    Group = "Образование",
    ShowInMenu = true,
    Description = "Управление академическими периодами")]
public class AcademicPeriodViewModel : RoutableViewModelBase
{
    #region Core Properties

    /// <summary>
    /// Связанная модель AcademicPeriod
    /// </summary>
    [Reactive] public AcademicPeriod AcademicPeriod { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string DisplayName { get; set; } = string.Empty;
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public DateTime StartDate { get; set; }
    [Reactive] public DateTime EndDate { get; set; }
    [Reactive] public AcademicPeriodType Type { get; set; }
    [Reactive] public AcademicPeriodStatus Status { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    #endregion

    #region Selection and UI Properties

    /// <summary>
    /// Коллекция академических периодов
    /// </summary>
    [Reactive] public ObservableCollection<AcademicPeriodViewModel> AcademicPeriods { get; set; } = new();
    
    /// <summary>
    /// Удалена ли запись (мягкое удаление)
    /// </summary>
    [Reactive] public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Выбранный академический период
    /// </summary>
    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public AcademicPeriodViewModel? SelectedItem { get; set; }
    [Reactive] public int CurrentPage { get; set; } = 1;

    public ObservableCollection<AcademicPeriodViewModel> Items => AcademicPeriods;

    public ReactiveCommand<Unit, Unit> CreatePeriodCommand { get; private set; } = null!;
    public ReactiveCommand<AcademicPeriodViewModel, Unit> EditCommand { get; private set; } = null!;
    public ReactiveCommand<AcademicPeriodViewModel, Unit> DeleteCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> PreviousPageCommand { get; private set; } = null!;
    public ReactiveCommand<Unit, Unit> NextPageCommand { get; private set; } = null!;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Full period name with type and dates
    /// </summary>
    public string FullName => $"{Name} ({TypeText})";

    /// <summary>
    /// Period type display
    /// </summary>
    public string TypeText => Type switch
    {
        AcademicPeriodType.Semester => "Семестр",
        AcademicPeriodType.Quarter => "Четверть",
        AcademicPeriodType.Trimester => "Триместр",
        AcademicPeriodType.Module => "Модуль",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Period status display
    /// </summary>
    public string StatusText => Status switch
    {
        AcademicPeriodStatus.Planned => "Запланирован",
        AcademicPeriodStatus.Active => "Активен",
        AcademicPeriodStatus.Completed => "Завершен",
        AcademicPeriodStatus.Cancelled => "Отменен",
        _ => "Неизвестно"
    };

    /// <summary>
    /// Status color
    /// </summary>
    public string StatusColor => Status switch
    {
        AcademicPeriodStatus.Planned => "#9E9E9E",
        AcademicPeriodStatus.Active => "#4CAF50",
        AcademicPeriodStatus.Completed => "#2196F3",
        AcademicPeriodStatus.Cancelled => "#F44336",
        _ => "#9E9E9E"
    };

    /// <summary>
    /// Period details
    /// </summary>
    public string PeriodDetails => $"{StartDate:d} - {EndDate:d}";

    /// <summary>
    /// Is period current
    /// </summary>
    public bool IsCurrent => DateTime.Now >= StartDate && DateTime.Now <= EndDate;

    /// <summary>
    /// Current status text
    /// </summary>
    public string CurrentStatusText => IsCurrent ? "Текущий" : "Не текущий";

    /// <summary>
    /// Current status color
    /// </summary>
    public string CurrentStatusColor => IsCurrent ? "#4CAF50" : "#9E9E9E";

    #endregion

    #region Constructor

    private readonly IAcademicPeriodService _academicPeriodService;
    private readonly ICourseInstanceService _courseInstanceService;
    private readonly INotificationService _notificationService;
    private readonly IDialogService? _dialogService;
    private readonly ILogger<AcademicPeriodViewModel> _logger;

    /// <summary>
    /// Creates the routable list-page AcademicPeriodViewModel (the "academic-periods" route)
    /// </summary>
    public AcademicPeriodViewModel(
        IScreen hostScreen,
        IAcademicPeriodService academicPeriodService,
        INotificationService notificationService,
        IDialogService dialogService,
        ILogger<AcademicPeriodViewModel> logger)
        : base(hostScreen)
    {
        _academicPeriodService = academicPeriodService;
        _notificationService = notificationService;
        _dialogService = dialogService;
        _logger = logger;

        InitializeCommands();
        _ = LoadAcademicPeriodsAsync();
    }

    /// <summary>
    /// Конструктор с периодом
    /// </summary>
    public AcademicPeriodViewModel(AcademicPeriod period, 
        IScreen hostScreen,
        IUnifiedNavigationService navigationService, 
        INotificationService notificationService,
        IAcademicPeriodService academicPeriodService,
        ICourseInstanceService courseInstanceService) 
        : base(null)
    {
        _academicPeriodService = academicPeriodService;
        _notificationService = notificationService;
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<AcademicPeriodViewModel>.Instance;

        if (period != null)
        {
            // Uid must be set before UpdateFromAcademicPeriod - it guards on period.Uid == Uid
            // to avoid overwriting the wrong tracked instance, and defaults to Guid.Empty otherwise.
            Uid = period.Uid;
            UpdateFromAcademicPeriod(period);
        }
    }

    private void InitializeCommands()
    {
        CreatePeriodCommand = CreateCommand(async () => await CreatePeriodAsync(), null, "Ошибка создания академического периода");
        EditCommand = CreateCommand<AcademicPeriodViewModel>(async (item) => await EditPeriodAsync(item), null, "Ошибка редактирования академического периода");
        DeleteCommand = CreateCommand<AcademicPeriodViewModel>(async (item) => await DeletePeriodAsync(item), null, "Ошибка удаления академического периода");
        PreviousPageCommand = CreateCommand(async () => { CurrentPage = Math.Max(1, CurrentPage - 1); await Task.CompletedTask; });
        NextPageCommand = CreateCommand(async () => { CurrentPage++; await Task.CompletedTask; });
    }

    private async Task CreatePeriodAsync()
    {
        if (_dialogService == null) return;

        var name = await _dialogService.ShowTextInputDialogAsync(
            "Новый академический период", "Введите название периода (например, «Осень 2026»)");
        if (string.IsNullOrWhiteSpace(name)) return;

        var newPeriod = new AcademicPeriod
        {
            Uid = Guid.NewGuid(),
            Name = name,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today.AddMonths(4),
            Type = AcademicPeriodType.Semester,
            Status = AcademicPeriodStatus.Planned,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _academicPeriodService.CreateAsync(newPeriod);
        var itemViewModel = new AcademicPeriodViewModel(created, HostScreen!, null!, _notificationService, _academicPeriodService, null!);
        AcademicPeriods.Add(itemViewModel);
        ShowSuccess($"Академический период «{created.Name}» создан");
    }

    private async Task EditPeriodAsync(AcademicPeriodViewModel item)
    {
        if (item == null || _dialogService == null) return;

        var newName = await _dialogService.ShowTextInputDialogAsync(
            "Редактирование периода", "Название периода", item.Name);
        if (string.IsNullOrWhiteSpace(newName)) return;

        var period = item.ToAcademicPeriod();
        period.Name = newName;
        await _academicPeriodService.UpdateAsync(period);
        item.Name = newName;
        ShowSuccess($"Академический период «{newName}» обновлён");
    }

    private async Task DeletePeriodAsync(AcademicPeriodViewModel item)
    {
        if (item == null || _dialogService == null) return;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Подтверждение удаления", $"Вы уверены, что хотите удалить период «{item.Name}»?");
        if (!confirmed) return;

        await _academicPeriodService.DeleteAsync(item.Uid);
        AcademicPeriods.Remove(item);
        ShowSuccess($"Академический период «{item.Name}» удалён");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Loads academic periods from the service
    /// </summary>
    private async Task LoadAcademicPeriodsAsync()
    {
        try
        {
            IsLoading = true;
            var periods = await _academicPeriodService.GetAllAsync();

            AcademicPeriods.Clear();
            foreach (var period in periods)
            {
                AcademicPeriods.Add(new AcademicPeriodViewModel(period, HostScreen!, null!, _notificationService, _academicPeriodService, null!));
            }

            StatusLogger.LogInfo($"Loaded {AcademicPeriods.Count} academic periods", "AcademicPeriodViewModel");
        }
        catch (Exception ex)
        {
            StatusLogger.LogError($"Error loading academic periods: {ex.Message}", "AcademicPeriodViewModel");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.Name, x => x.Type)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(FullName));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Type)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(TypeText));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(StatusColor));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.StartDate, x => x.EndDate)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(PeriodDetails));
                this.RaisePropertyChanged(nameof(IsCurrent));
                this.RaisePropertyChanged(nameof(CurrentStatusText));
                this.RaisePropertyChanged(nameof(CurrentStatusColor));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from an AcademicPeriod domain model
    /// </summary>
    public void UpdateFromAcademicPeriod(AcademicPeriod period)
    {
        if (period == null)
            throw new ArgumentNullException(nameof(period));
            
        if (period.Uid != Uid)
            throw new ArgumentException("Cannot update from period with different UID", nameof(period));
            
        AcademicPeriod = period;
        Uid = period.Uid;
        Name = period.Name;
        Description = period.Description;
        StartDate = period.StartDate;
        EndDate = period.EndDate;
        Type = period.Type;
        Status = period.Status;
        IsActive = period.IsActive;
        CreatedAt = period.CreatedAt;
        LastModifiedAt = period.LastModifiedAt;
    }

    /// <summary>
    /// Converts this ViewModel back to an AcademicPeriod domain model
    /// </summary>
    public AcademicPeriod ToAcademicPeriod()
    {
        var period = new AcademicPeriod
        {
            Uid = Uid,
            Name = Name,
            Description = Description,
            StartDate = StartDate,
            EndDate = EndDate,
            Type = Type,
            Status = Status,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        return period;
    }

    /// <summary>
    /// Creates a copy of this AcademicPeriodViewModel
    /// </summary>
    public AcademicPeriodViewModel Clone()
    {
        return new AcademicPeriodViewModel(ToAcademicPeriod(), HostScreen!, null!, _notificationService, _academicPeriodService, null!);
    }

    /// <summary>
    /// Validates the academic period data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Необходимо указать название периода");
            
        if (EndDate <= StartDate)
            errors.Add("Дата окончания должна быть позже даты начала");
            
        // Business logic validation
        if (Status == AcademicPeriodStatus.Active && !IsCurrent)
            warnings.Add("Активный период не является текущим");
            
        if (Status == AcademicPeriodStatus.Completed && EndDate > DateTime.Now)
            warnings.Add("Завершенный период еще не закончился");
        
        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    #endregion
} 

