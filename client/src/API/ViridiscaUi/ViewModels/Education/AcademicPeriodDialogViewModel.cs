using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Navigations;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Domain.Entities.Education.Enums;
using ViridiscaUi.Infrastructure.Logger;

namespace ViridiscaUi.ViewModels.Education;

/// <summary>
/// ViewModel для диалога создания/редактирования академического периода
/// </summary>
public class AcademicPeriodDialogViewModel : ViewModelBase 
{
    #region Properties

    [Reactive] public string Name { get; set; } = string.Empty;
    [Reactive] public string Code { get; set; } = string.Empty;
    [Reactive] public AcademicPeriodType Type { get; set; }
    [Reactive] public string Status { get; set; } = "Активный";
    [Reactive] public DateTime StartDate { get; set; } = DateTime.Now;
    [Reactive] public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(4);
    [Reactive] public bool IsActive { get; set; } = true;
    [Reactive] public string Description { get; set; } = string.Empty;

    #endregion

    #region Collections

    [Reactive] public ObservableCollection<AcademicPeriodType> AvailableTypes { get; set; } = new();
    [Reactive] public ObservableCollection<string> AvailableStatuses { get; set; } = new();

    #endregion

    private readonly IAcademicPeriodService _academicPeriodService; 
    
    protected AcademicPeriod? Entity { get; set; }

    #region Constructor

    public AcademicPeriodDialogViewModel( 
        IAcademicPeriodService academicPeriodService,
        AcademicPeriod? entity = null,
        string? dialogTitle = null)
    { 
        _academicPeriodService = academicPeriodService ?? throw new ArgumentNullException(nameof(academicPeriodService));

        Entity = entity;
        Title = dialogTitle ?? (entity != null ? "Редактирование академического периода" : "Создание академического периода");

        // Инициализация значений по умолчанию
        Name = string.Empty;
        Code = string.Empty;
        Description = string.Empty;
        IsActive = true;
        Type = AcademicPeriodType.Semester;

        // Загрузка доступных типов
        LoadAvailableTypes();
        LoadAvailableStatuses();

        // Инициализация из существующей сущности
        if (entity != null)
        {
            InitializeFromEntity(entity);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Инициализирует ViewModel из сущности AcademicPeriod
    /// </summary>
    protected void InitializeFromEntity(AcademicPeriod entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        Name = entity.Name;
        Code = entity.Code;
        Type = entity.Type;
        StartDate = entity.StartDate;
        EndDate = entity.EndDate;
        IsActive = entity.IsActive;
        Description = entity.Description ?? string.Empty;
    }

    /// <summary>
    /// Валидирует данные академического периода
    /// </summary>
    protected void Validate()
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(Name))
            errors.Add("Название академического периода обязательно для заполнения");

        if (string.IsNullOrWhiteSpace(Code))
            errors.Add("Код академического периода обязателен для заполнения");

        if (StartDate >= EndDate)
            errors.Add("Дата начала должна быть раньше даты окончания");

        if (StartDate < DateTime.Now.AddYears(-10))
            errors.Add("Дата начала не может быть более 10 лет назад");

        if (EndDate > DateTime.Now.AddYears(10))
            errors.Add("Дата окончания не может быть более чем через 10 лет");

        ValidationError = errors.Count > 0 ? string.Join("; ", errors) : null;
    }

    /// <summary>
    /// Сохраняет данные в сущность AcademicPeriod
    /// </summary>
    protected async Task<AcademicPeriod?> SaveEntityAsync()
    {
        try
        {
            var entity = Entity ?? new AcademicPeriod();
            
            entity.Name = Name;
            entity.Code = Code;
            entity.Type = Type;
            entity.StartDate = StartDate;
            entity.EndDate = EndDate;
            entity.IsActive = IsActive;
            entity.Description = Description;
            
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
            StatusLogger.LogError($"Ошибка при сохранении академического периода: {ex.Message}", "AcademicPeriodDialog");
            return null;
        }
    }

    /// <summary>
    /// Загружает доступные типы академических периодов
    /// </summary>
    private void LoadAvailableTypes()
    {
        AvailableTypes.Clear();
        
        foreach (AcademicPeriodType type in Enum.GetValues<AcademicPeriodType>())
        {
            AvailableTypes.Add(type);
        }
    }

    /// <summary>
    /// Загружает доступные статусы академических периодов
    /// </summary>
    private void LoadAvailableStatuses()
    {
        AvailableStatuses.Clear();
        AvailableStatuses.Add("Активный");
        AvailableStatuses.Add("Неактивный");
        AvailableStatuses.Add("Завершенный");
        AvailableStatuses.Add("Планируемый");
    }

    #endregion
} 

