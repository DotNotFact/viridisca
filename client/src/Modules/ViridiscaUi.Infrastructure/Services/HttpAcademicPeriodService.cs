using Microsoft.Extensions.Logging;
using ViridiscaUi.Domain.Entities.Education;
using ViridiscaUi.Domain.Services.Education;
using ViridiscaUi.Infrastructure.ApiClient;

namespace ViridiscaUi.Infrastructure.Services;

/// <summary>
/// HTTP-backed IAcademicPeriodService — the backend covers nearly the whole interface
/// (it's the cleanest, most CRUD-shaped of the four Curriculum services). Only
/// DeleteAsync (no DELETE endpoint) falls through to <paramref name="inner"/>.
/// </summary>
public class HttpAcademicPeriodService(AcademicPeriodApiClient apiClient, AcademicPeriodService inner, ILogger<HttpAcademicPeriodService> logger) : IAcademicPeriodService
{
    private readonly AcademicPeriodApiClient _apiClient = apiClient;
    private readonly AcademicPeriodService _inner = inner;
    private readonly ILogger<HttpAcademicPeriodService> _logger = logger;

    public async Task<IEnumerable<AcademicPeriod>> GetAllAsync()
    {
        var (success, data, error) = await _apiClient.GetAllAsync();
        if (!success || data is null)
        {
            _logger.LogWarning("GetAllAsync (academic periods) failed: {Error}", error);
            return [];
        }

        return data.Select(CurriculumMappers.ToAcademicPeriod);
    }

    public Task<AcademicPeriod?> GetByIdAsync(Guid uid) => GetByUidAsync(uid);

    public async Task<AcademicPeriod?> GetByUidAsync(Guid uid)
    {
        var (success, data, error) = await _apiClient.GetAsync(uid);
        if (!success || data is null)
        {
            _logger.LogWarning("GetByUidAsync({PeriodUid}) failed: {Error}", uid, error);
            return null;
        }

        return CurriculumMappers.ToAcademicPeriod(data);
    }

    public async Task<AcademicPeriod?> GetCurrentAsync()
    {
        var (success, data, _) = await _apiClient.GetCurrentAsync();
        return success && data is not null ? CurriculumMappers.ToAcademicPeriod(data) : null;
    }

    public async Task<IEnumerable<AcademicPeriod>> GetActiveAsync()
        => (await GetAllAsync()).Where(p => p.Status == Domain.Entities.Education.Enums.AcademicPeriodStatus.Active);

    public async Task<AcademicPeriod> CreateAsync(AcademicPeriod academicPeriod)
    {
        var request = new CreateAcademicPeriodRequestDto(
            academicPeriod.Name, academicPeriod.Code, academicPeriod.StartDate, academicPeriod.EndDate,
            academicPeriod.AcademicYear, academicPeriod.Type.ToString(), academicPeriod.Description ?? string.Empty);

        var (success, periodUid, error) = await _apiClient.CreateAsync(request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to create academic period: {error}");
        }

        academicPeriod.Uid = periodUid;
        return academicPeriod;
    }

    public async Task<AcademicPeriod> UpdateAsync(AcademicPeriod academicPeriod)
    {
        var request = new UpdateAcademicPeriodRequestDto(academicPeriod.Name, academicPeriod.Description ?? string.Empty, academicPeriod.StartDate, academicPeriod.EndDate);
        var (success, error) = await _apiClient.UpdateAsync(academicPeriod.Uid, request);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to update academic period: {error}");
        }

        return academicPeriod;
    }

    public Task<bool> DeleteAsync(Guid uid)
    {
        _logger.LogWarning("DeleteAsync is not supported by the backend yet (no DELETE endpoint for academic periods)");
        return _inner.DeleteAsync(uid);
    }

    public async Task<bool> ExistsAsync(Guid uid) => await GetByUidAsync(uid) is not null;

    public async Task<int> GetCountAsync() => (await GetAllAsync()).Count();
}
