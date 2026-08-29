using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.Logger;
using ViridiscaUi.ViewModels.Bases;
using ViridiscaUi.Navigations;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Models.Base;

namespace ViridiscaUi.ViewModels.Education
{
    /// <summary>
    /// ViewModel для списка студентов с CRUD операциями
    /// </summary>
    /// <remarks>
    /// Was previously a second, distinct-path "students" route with no ReactiveViewLocator
    /// mapping - a phantom sidebar entry (defaulted into the "Основное" group since Group
    /// wasn't set) that froze the screen when clicked. StudentsViewModel ("students" route)
    /// is the real, wired page. Not a page - no [Route] here.
    /// </remarks>
    public class StudentListViewModel : RoutableViewModelBase
    {
        private readonly IStudentService _studentService;
        private readonly ILogger<StudentListViewModel> _logger;

        [Reactive] public ObservableCollection<Student> Students { get; set; } = [];
        [Reactive] public Student? SelectedStudent { get; set; }
        [Reactive] public string SearchText { get; set; } = string.Empty;
        [Reactive] public bool IsLoading { get; set; }
        [Reactive] public int CurrentPage { get; set; } = 1;
        [Reactive] public int PageSize { get; set; } = 20;

        public ReactiveCommand<Unit, Unit> LoadStudentsCommand { get; }
        public ReactiveCommand<Unit, Unit> RefreshCommand { get; }

        public StudentListViewModel(
            IScreen hostScreen,
            IStudentService studentService,
            ILogger<StudentListViewModel> logger) : base(hostScreen)
        {
            _studentService = studentService;
            _logger = logger;

            LoadStudentsCommand = ReactiveCommand.CreateFromTask(LoadStudentsAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadStudentsAsync);

            // Setup reactive subscriptions
            this.WhenAnyValue(x => x.SearchText)
                .Throttle(TimeSpan.FromMilliseconds(300))
                .DistinctUntilChanged()
                .Subscribe(_ => LoadStudentsCommand.Execute().Subscribe(_ => { }, _ => { }));
        }

        /// <summary>
        /// Загружает список студентов
        /// </summary>
        private async Task LoadStudentsAsync()
        {
            try
            {
                IsLoading = true;
                var (students, totalCount) = await _studentService.GetPagedAsync(CurrentPage, PageSize, SearchText);
                
                Students.Clear();
                foreach (var student in students)
                {
                    Students.Add(student);
                }

                StatusLogger.LogInfo($"Loaded {Students.Count} students");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading students");
                StatusLogger.LogError($"Ошибка загрузки студентов: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override async Task OnFirstTimeLoadedAsync()
        {
            await LoadStudentsCommand.Execute();
        }
    }
} 

