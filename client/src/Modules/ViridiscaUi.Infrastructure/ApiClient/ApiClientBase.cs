using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace ViridiscaUi.Infrastructure.ApiClient;

/// <summary>
/// Shared HTTP plumbing for the thin per-module API clients (IdentityApiClient,
/// StudentApiClient, etc.) — request/response JSON handling and ProblemDetails error
/// extraction, factored out so each client only has to declare its own endpoints.
/// </summary>
public abstract class ApiClientBase(HttpClient httpClient, ILogger logger)
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    protected readonly HttpClient HttpClient = httpClient;
    private readonly ILogger _logger = logger;

    protected Task<(bool Success, TResponse? Data, string? Error)> PostAsync<TRequest, TResponse>(
        string url, TRequest request, CancellationToken cancellationToken, bool expectBody = true)
        => SendAsync<TResponse>(() => HttpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken), url, cancellationToken, expectBody);

    protected Task<(bool Success, TResponse? Data, string? Error)> PutAsync<TRequest, TResponse>(
        string url, TRequest request, CancellationToken cancellationToken, bool expectBody = true)
        => SendAsync<TResponse>(() => HttpClient.PutAsJsonAsync(url, request, JsonOptions, cancellationToken), url, cancellationToken, expectBody);

    protected Task<(bool Success, TResponse? Data, string? Error)> GetAsync<TResponse>(
        string url, CancellationToken cancellationToken)
        => SendAsync<TResponse>(() => HttpClient.GetAsync(url, cancellationToken), url, cancellationToken);

    protected async Task<(bool Success, string? Error)> DeleteAsync(string url, CancellationToken cancellationToken)
    {
        var (success, _, error) = await SendAsync<object>(() => HttpClient.DeleteAsync(url, cancellationToken), url, cancellationToken, expectBody: false);
        return (success, error);
    }

    private async Task<(bool Success, TResponse? Data, string? Error)> SendAsync<TResponse>(
        Func<Task<HttpResponseMessage>> send, string url, CancellationToken cancellationToken, bool expectBody = true)
    {
        try
        {
            HttpResponseMessage response = await send();
            if (!response.IsSuccessStatusCode)
            {
                return (false, default, await ExtractErrorAsync(response, cancellationToken));
            }

            if (!expectBody)
            {
                return (true, default, null);
            }

            TResponse? data = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
            return (true, data, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request to {Url} failed", url);
            return (false, default, ex.Message);
        }
    }

    private static async Task<string> ExtractErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsDto>(JsonOptions, cancellationToken);
            return problem?.Detail ?? problem?.Title ?? $"Request failed with status {(int)response.StatusCode}";
        }
        catch
        {
            return $"Request failed with status {(int)response.StatusCode}";
        }
    }

    private sealed record ProblemDetailsDto(string? Title, string? Detail);
}
