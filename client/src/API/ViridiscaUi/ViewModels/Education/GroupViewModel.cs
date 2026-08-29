using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using DomainGroup = ViridiscaUi.Domain.Entities.Education.Group;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// Enhanced ViewModel for individual group with full reactive support
/// Supports selection, computed properties, and data binding
/// </summary>
[Route("group-details", 
    DisplayName = "Детали группы", 
    IconKey = "AccountMultiple", 
    Order = 999,
    Group = "Образование",
    ShowInMenu = false,
    Description = "Детальная информация о группе")]
public class GroupViewModel : ViewModelBase
{
    #region Core Properties

    /// <summary>
    /// Связанная модель Group
    /// </summary>
    [Reactive] public DomainGroup Group { get; set; } = new();

    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public string Description { get; set; } = string.Empty;
    [Reactive] public int Year { get; set; }
    [Reactive] public GroupStatus Status { get; set; }
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }
    [Reactive] public string Title { get; set; } = "Группа";

    #endregion

    #region Department Properties

    [Reactive] public Guid DepartmentUid { get; set; }
    [Reactive] public string DepartmentName { get; set; } = string.Empty;
    [Reactive] public string DepartmentCode { get; set; } = string.Empty;

    #endregion

    #region Curator Properties

    [Reactive] public Guid? CuratorUid { get; set; }
    [Reactive] public string? CuratorName { get; set; }
    [Reactive] public string? CuratorInitials { get; set; }
    [Reactive] public string? CuratorSpecialization { get; set; }
    [Reactive] public int StudentsCount { get; set; }

    #endregion

    #region Selection and UI Properties

    [Reactive] public bool IsSelected { get; set; }
    [Reactive] public bool IsLoading { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Group details
    /// </summary>
    public string GroupDetails => $"{Name} ({Code})";

    /// <summary>
    /// Department details
    /// </summary>
    public string DepartmentDetails => $"{DepartmentName} ({DepartmentCode})";

    /// <summary>
    /// Status color
    /// </summary>
    public string StatusColor => Status switch
    {
        GroupStatus.Active => "#4CAF50",
        GroupStatus.Graduated => "#2196F3",
        GroupStatus.Disbanded => "#F44336",
        _ => "#9E9E9E"
    };

    #endregion

    #region Initialization

    private void InitializeComputedProperties()
    {
        // Initialize computed properties for reactive UI
        this.WhenAnyValue(x => x.Status)
            .Select(status => status switch
            {
                GroupStatus.Active => "Активна",
                GroupStatus.Graduated => "Выпущена",
                GroupStatus.Disbanded => "Расформирована",
                _ => "Неизвестно"
            })
            .ToPropertyEx(this, x => x.StatusText);

        this.WhenAnyValue(x => x.Name, x => x.Code)
            .Select(tuple => $"{tuple.Item1} ({tuple.Item2})")
            .ToPropertyEx(this, x => x.DisplayName);
    }

    #endregion

    #region Reactive Properties

    public extern string StatusText { [ObservableAsProperty] get; }
    public extern string DisplayName { [ObservableAsProperty] get; }

    #endregion

    #region Constructor

    private readonly IGroupService _groupService;

    /// <summary>
    /// Creates a GroupViewModel from a Group domain model
    /// </summary>
    public GroupViewModel(
        IScreen hostScreen,
        IUnifiedNavigationService navigationService,
        IGroupService groupService) 
        : base()
    {
        _groupService = groupService ?? throw new ArgumentNullException(nameof(groupService));
        
        Title = "Группа";
        
        // Initialize computed properties
        InitializeComputedProperties();
    }

    /// <summary>
    /// Простой конструктор для использования в коллекциях
    /// </summary>
    public GroupViewModel(DomainGroup group) : base()
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));

        // Uid must be set before UpdateFromGroup - it guards on group.Uid == Uid to avoid
        // overwriting the wrong tracked instance, and defaults to Guid.Empty otherwise.
        Uid = group.Uid;
        UpdateFromGroup(group);
        SetupPropertyChangeNotifications();
    }

    /// <summary>
    /// Конструктор с группой и сервисом
    /// </summary>
    public GroupViewModel(DomainGroup group, IGroupService groupService)
        : this(null!, null!, groupService)
    {
        Uid = group.Uid;
        UpdateFromGroup(group);
    }

    /// <summary>
    /// Простой конструктор для создания пустой ViewModel
    /// </summary>
    public GroupViewModel() 
        : this(null!, null!, null!)
    {
        // Пустой конструктор для создания новых групп
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Sets up reactive property change notifications for computed properties
    /// </summary>
    private void SetupPropertyChangeNotifications()
    {
        // Notify when computed properties should update
        this.WhenAnyValue(x => x.Name, x => x.Code)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(GroupDetails));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.DepartmentName, x => x.DepartmentCode)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(DepartmentDetails));
            })
            .DisposeWith(Disposables);
            
        this.WhenAnyValue(x => x.Status)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(StatusColor));
            })
            .DisposeWith(Disposables);
    }

    /// <summary>
    /// Updates this ViewModel from a Group domain model
    /// </summary>
    public void UpdateFromGroup(DomainGroup group)
    {
        if (group == null)
            throw new ArgumentNullException(nameof(group));
            
        if (group.Uid != Uid)
            throw new ArgumentException("Cannot update from group with different UID", nameof(group));
            
        Group = group;
        Uid = group.Uid;
        Name = group.Name;
        Code = group.Code;
        Description = group.Description;
        Year = group.Year;
        Status = group.Status;
        IsActive = group.IsActive;
        
        // Получаем данные из связанной модели Department
        if (group.Department != null)
        {
            DepartmentUid = group.Department.Uid;
            DepartmentName = group.Department.Name;
            DepartmentCode = group.Department.Code;
        }
        
        // Получаем данные из связанной модели Curator
        CuratorUid = group.CuratorUid;
        if (group.Curator?.Person != null)
        {
            CuratorName = $"{group.Curator.Person.FirstName} {group.Curator.Person.LastName}";
            CuratorInitials = $"{group.Curator.Person.FirstName.FirstOrDefault()}{group.Curator.Person.LastName.FirstOrDefault()}";
            CuratorSpecialization = group.Curator.Specialization ?? "Не указано";
        }
        else
        {
            CuratorName = null;
            CuratorInitials = null;
            CuratorSpecialization = null;
        }
        
        // Получаем количество студентов
        StudentsCount = group.Students?.Count ?? 0;
        
        CreatedAt = group.CreatedAt;
        LastModifiedAt = group.LastModifiedAt;
    }

    /// <summary>
    /// Converts this ViewModel back to a Group domain model
    /// </summary>
    public DomainGroup ToGroup()
    {
        var group = new DomainGroup
        {
            Uid = Uid,
            Name = Name,
            Code = Code,
            Description = Description,
            Year = Year,
            Status = Status,
            IsActive = IsActive,
            DepartmentUid = DepartmentUid,
            CuratorUid = CuratorUid,
            CreatedAt = CreatedAt,
            LastModifiedAt = DateTime.UtcNow
        };

        return group;
    }

    /// <summary>
    /// Creates a copy of this GroupViewModel
    /// </summary>
    public GroupViewModel Clone()
    {
        var cloned = new GroupViewModel();
        cloned.Uid = Uid;
        cloned.Name = Name;
        cloned.Code = Code;
        cloned.Description = Description;
        cloned.Year = Year;
        cloned.Status = Status;
        cloned.IsActive = IsActive;
        cloned.DepartmentUid = DepartmentUid;
        cloned.DepartmentName = DepartmentName;
        cloned.DepartmentCode = DepartmentCode;
        cloned.CreatedAt = CreatedAt;
        cloned.LastModifiedAt = LastModifiedAt;
        return cloned;
    }

    /// <summary>
    /// Validates the group data
    /// </summary>
    public DomainValidationResult Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Необходимо указать название группы");
            
        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Необходимо указать код группы");
            
        // Business logic validation
        if (Year < 2000 || Year > DateTime.Now.Year + 1)
            errors.Add("Некорректный год группы");
            
        if (Status == GroupStatus.Graduated && IsActive)
            warnings.Add("Выпущенная группа не может быть активной");
        
        return new DomainValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    #endregion

    #region Equality and Comparison

    /// <summary>
    /// Returns a string representation of the group
    /// </summary>
    public override string ToString()
    {
        return $"{Name} ({Code}) - {StatusText}";
    }

    /// <summary>
    /// Determines equality based on UID
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is GroupViewModel other && Uid.Equals(other.Uid);
    }

    /// <summary>
    /// Returns hash code based on UID
    /// </summary>
    public override int GetHashCode()
    {
        return Uid.GetHashCode();
    }

    #endregion
} 

