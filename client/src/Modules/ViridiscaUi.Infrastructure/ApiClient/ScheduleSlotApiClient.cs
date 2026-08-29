using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

public class ScheduleSlotApiClient(HttpClient httpClient, ILogger<ScheduleSlotApiClient> logger)
    : ApiClientBase(httpClient, logger)
{
    public Task<(bool Success, Guid Data, string? Error)> CreateAsync(CreateScheduleSlotRequestDto request, CancellationToken cancellationToken = default)
        => PostAsync<CreateScheduleSlotRequestDto, Guid>("api/scheduler/schedule-slots", request, cancellationToken);

    public Task<(bool Success, ScheduleSlotResponseDto? Data, string? Error)> GetAsync(Guid scheduleSlotUid, CancellationToken cancellationToken = default)
        => GetAsync<ScheduleSlotResponseDto>($"api/scheduler/schedule-slots/{scheduleSlotUid}", cancellationToken);

    public Task<(bool Success, List<ScheduleSlotResponseDto>? Data, string? Error)> GetAllAsync(CancellationToken cancellationToken = default)
        => GetAsync<List<ScheduleSlotResponseDto>>("api/scheduler/schedule-slots", cancellationToken);

    public Task<(bool Success, List<ScheduleSlotResponseDto>? Data, string? Error)> GetByCourseInstanceAsync(Guid courseInstanceUid, CancellationToken cancellationToken = default)
        => GetAsync<List<ScheduleSlotResponseDto>>($"api/scheduler/schedule-slots/by-course-instance/{courseInstanceUid}", cancellationToken);

    public Task<(bool Success, List<ScheduleSlotResponseDto>? Data, string? Error)> GetUpcomingAsync(int count, CancellationToken cancellationToken = default)
        => GetAsync<List<ScheduleSlotResponseDto>>($"api/scheduler/schedule-slots/upcoming?count={count}", cancellationToken);

    public async Task<(bool Success, string? Error)> UpdateAsync(Guid scheduleSlotUid, UpdateScheduleSlotRequestDto request, CancellationToken cancellationToken = default)
    {
        var (success, _, error) = await PutAsync<UpdateScheduleSlotRequestDto, object>($"api/scheduler/schedule-slots/{scheduleSlotUid}", request, cancellationToken, expectBody: false);
        return (success, error);
    }

    public Task<(bool Success, string? Error)> DeleteAsync(Guid scheduleSlotUid, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/scheduler/schedule-slots/{scheduleSlotUid}", cancellationToken);
}
