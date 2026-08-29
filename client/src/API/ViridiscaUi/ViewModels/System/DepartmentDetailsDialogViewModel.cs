using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.System;
using ViridiscaUi.Domain.Services.System;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.ViewModels.System;

/// <summary>
/// ViewModel для диалога просмотра деталей департамента
/// </summary>
public class DepartmentDetailsDialogViewModel : ViewModelBase, IActivatableViewModel
{
    private readonly IDepartmentService _departmentService;
    
    [Reactive] public Department? Department { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public string Title { get; set; }

    public ReactiveCommand<Unit, bool> CloseCommand { get; private set; }

    public DepartmentDetailsDialogViewModel(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
        Activator = new ViewModelActivator();
        
        this.WhenActivated(disposables =>
        {
            // Активация логики
            Disposable.Create(() => { }).DisposeWith(disposables);
        });

        CloseCommand = ReactiveCommand.Create(() => true);
    }

    public async Task LoadDepartmentAsync(Guid departmentUid)
    {
        IsLoading = true;
        
        Department = await _departmentService.GetDepartmentAsync(departmentUid);
        
        IsLoading = false;
    }

    public ViewModelActivator Activator { get; }
} 

