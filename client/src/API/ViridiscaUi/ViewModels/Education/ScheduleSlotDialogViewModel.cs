using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Base;
using ViridiscaUi.Navigations;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования слота расписания
/// </summary>
public class ScheduleSlotDialogViewModel : RoutableViewModelBase
{
    #region Properties

    [Reactive] public DayOfWeek DayOfWeek { get; set; }
    [Reactive] public TimeSpan StartTime { get; set; }
    [Reactive] public TimeSpan EndTime { get; set; }
    [Reactive] public string Location { get; set; } = string.Empty;
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public Group? Group { get; set; }

    #endregion

    #region Collections

    [Reactive] public ObservableCollection<Group> AvailableGroups { get; set; } = new();

    #endregion

    #region Constructor

    public ScheduleSlotDialogViewModel( 
        ScheduleSlot? scheduleSlot = null,
        string title = "Слот расписания")
        : base(null)
    {
        // Загружаем доступные группы
        LoadAvailableDataAsync().ConfigureAwait(false);
        
        // Если передан существующий слот, инициализируем из него
        if (scheduleSlot != null)
        {
            InitializeFromEntity(scheduleSlot);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Инициализирует ViewModel из сущности ScheduleSlot
    /// </summary>
    protected void InitializeFromEntity(ScheduleSlot scheduleSlot)
    {
        DayOfWeek = scheduleSlot.DayOfWeek;
        StartTime = scheduleSlot.StartTime;
        EndTime = scheduleSlot.EndTime;
        Location = scheduleSlot.Location;
        IsActive = scheduleSlot.IsActive;
        
        // Загружаем связанную сущность
        if (scheduleSlot.Group != null)
        {
            Group = AvailableGroups.FirstOrDefault(g => g.Uid == scheduleSlot.Group.Uid);
        }
    }

    /// <summary>
    /// Валидирует данные слота расписания
    /// </summary>
    protected bool Validate()
    {
        var errors = new List<string>();
        var warnings = new List<string>();
        
        // Required field validation
        if (string.IsNullOrWhiteSpace(Location))
            errors.Add("Необходимо указать место проведения");
            
        if (Group == null)
            errors.Add("Необходимо выбрать группу");
            
        if (StartTime >= EndTime)
            errors.Add("Время начала должно быть раньше времени окончания");
        
        return errors.Count == 0;
    }

    /// <summary>
    /// Сохраняет данные в сущность ScheduleSlot
    /// </summary>
    protected void SaveEntity(ScheduleSlot entity)
    {
        entity.DayOfWeek = DayOfWeek;
        entity.StartTime = StartTime;
        entity.EndTime = EndTime;
        entity.Location = Location;
        entity.IsActive = IsActive;
        
        if (Group != null)
        {
            // GroupUid is computed from CourseInstance.GroupUid, so we don't set it directly
            // Instead, we need to ensure the CourseInstance has the correct GroupUid
        }
        
        entity.LastModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Загружает доступные группы
    /// </summary>
    private async Task LoadAvailableDataAsync()
    {
        try
        {
            // Здесь должна быть загрузка групп через сервис
            // var groups = await _groupService.GetAllAsync();
            // AvailableGroups.Clear();
            // AvailableGroups.AddRange(groups);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка при загрузке данных для диалога слота расписания");
        }
    }

    #endregion
} 

