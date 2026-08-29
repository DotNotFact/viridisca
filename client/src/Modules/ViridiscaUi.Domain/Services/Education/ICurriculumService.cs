using ViridiscaUi.Domain.Entities.Education;

namespace ViridiscaUi.Domain.Services.Education;

/// <summary>
/// Сервис для управления учебными планами
/// </summary>
public interface ICurriculumService
{
    Task<IEnumerable<Curriculum>> GetAllAsync();
    Task<Curriculum?> GetByIdAsync(Guid uid);
    Task<Curriculum?> GetByCodeAsync(string code);
    Task<Curriculum?> GetByUidAsync(Guid uid);
    Task<Curriculum> CreateAsync(Curriculum curriculum);
    Task<Curriculum> UpdateAsync(Curriculum curriculum);
    Task<bool> DeleteAsync(Guid uid);
    Task<bool> ExistsAsync(Guid uid);
    Task<int> GetCountAsync();
    Task<int> GetStudentsCountAsync(Guid curriculumUid);
    Task<int> GetSubjectsCountAsync(Guid curriculumUid);
    Task<Curriculum> CopyAsync(Guid curriculumUid, string? newName = null);
    Task<Curriculum> ActivateAsync(Guid curriculumUid);
    Task<Curriculum> DeactivateAsync(Guid curriculumUid);
    Task<byte[]> ExportAsync(Guid curriculumUid);
    Task<Curriculum?> ImportAsync();
    Task<(int TotalSubjects, int TotalCredits, int CompletedSubjects, decimal CompletionRate)> GetStatisticsAsync(Guid curriculumUid);
    
    // Missing methods needed by ViewModels
    /// <summary>
    /// Получает предметы учебного плана
    /// </summary>
    Task<IEnumerable<CurriculumSubject>> GetCurriculumSubjectsAsync(IEnumerable<Guid> curriculumUids);

    /// <summary>
    /// Дублирует учебный план
    /// </summary>
    Task<Curriculum> DuplicateCurriculumAsync(Guid curriculumUid, string newName);
    
    // Расширенные методы для пагинации и фильтрации
    Task<(IEnumerable<Curriculum> curricula, int totalCount)> GetPagedAsync(
        int page, 
        int pageSize, 
        string? searchTerm = null,
        Guid? departmentUid = null,
        bool? isActive = null,
        int? minCredits = null,
        int? maxCredits = null,
        int? academicYear = null);
} 