using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.ViewModels.Bases;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для диалога создания/редактирования департамента
/// </summary>
public class DepartmentEditDialogViewModel : ViewModelBase
{
    private readonly IDepartmentService _departmentService;
    private readonly bool _isEdit;

    [Reactive] public Department Department { get; set; }
    [Reactive] public string Title { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public string? ValidationError { get; set; }

    public ReactiveCommand<Unit, bool> SaveCommand { get; private set; }
    public ReactiveCommand<Unit, bool> CancelCommand { get; private set; }

    public DepartmentEditDialogViewModel(Department department, IDepartmentService departmentService, bool isEdit)
    {
        Department = department ?? throw new ArgumentNullException(nameof(department));
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
        _isEdit = isEdit;
        Title = isEdit ? "Редактирование департамента" : "Создание департамента";

        SetupCommands();
    }

    private void SetupCommands()
    {
        var canSave = this.WhenAnyValue(
            x => x.Department.Name,
            x => x.Department.Code,
            x => x.IsLoading,
            (name, code, loading) => !string.IsNullOrWhiteSpace(name) && 
                                   !string.IsNullOrWhiteSpace(code) && 
                                   !loading);

        SaveCommand = ReactiveCommand.CreateFromTask(SaveAsync, canSave);
        CancelCommand = ReactiveCommand.Create(() => false);
    }

    private async Task<bool> SaveAsync()
    {
        try
        {
            IsLoading = true;
            ValidationError = null;

            // Validate unique code
            var existsByCode = await _departmentService.ExistsByCodeAsync(Department.Code, _isEdit ? Department.Uid : null);
            if (existsByCode)
            {
                ValidationError = "Департамент с таким кодом уже существует";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            ValidationError = ex.Message;
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
} 

