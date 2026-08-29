using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

public class AcademicPeriodApiClient(HttpClient httpClient, ILogger<AcademicPeriodApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateAcademicPeriodRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateAcademicPeriodRequestDto, Guid>("api/curriculum/academic-periods", request, cancellationToken);

    public Task<(bool Success, AcademicPeriodResponseDto? Data, string? Error)> GetAsync(Guid periodUid, CancellationToken cancellationToken = default)
        => GetAsync<AcademicPeriodResponseDto>($"api/curriculum/academic-periods/{periodUid}", cancellationToken);

    public Task<(bool Success, List<AcademicPeriodResponseDto>? Data, string? Error)> GetAllAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<AcademicPeriodResponseDto>>("api/curriculum/academic-periods", cancellationToken);

    public Task<(bool Success, AcademicPeriodResponseDto? Data, string? Error)> GetCurrentAsync(CancellationToken cancellationToken = default)
        => GetAsync<AcademicPeriodResponseDto>("api/curriculum/academic-periods/current", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid periodUid, UpdateAcademicPeriodRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateAcademicPeriodRequestDto, object>($"api/curriculum/academic-periods/{periodUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public async Task<(bool Success, string? Error)> SetCurrentAsync(Guid periodUid, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<object?, object>($"api/curriculum/academic-periods/{periodUid}/current", null, cancellationToken, expectBody: false);
        return (success, error);
    }
}
