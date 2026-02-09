using SegmentationManual.Models;

namespace SegmentationManual.Services;

public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly bool _ownsHttpClient;

    public ApiClient(string baseUrl, HttpClient? httpClient = null)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _ownsHttpClient = httpClient == null;
        _httpClient = httpClient ?? new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<bool> HideFieldAsync(
        string dataSourceId,
        string fieldName,
        CancellationToken cancellationToken = default
    )
    {
        var endpoint = $"/api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Hide";
        return await ExecuteVisibilityRequestAsync(endpoint, cancellationToken);
    }

    public async Task<bool> UnhideFieldAsync(
        string dataSourceId,
        string fieldName,
        CancellationToken cancellationToken = default
    )
    {
        var endpoint = $"/api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Unhide";
        return await ExecuteVisibilityRequestAsync(endpoint, cancellationToken);
    }

    private async Task<bool> ExecuteVisibilityRequestAsync(
        string endpoint,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var url = $"{_baseUrl}{endpoint}";
            Console.WriteLine($"Executing: {url}");

            var request = new HttpRequestMessage(HttpMethod.Put, url);
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Success: {response.StatusCode}");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Error {response.StatusCode}: {errorContent}");
                return false;
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
            return false;
        }
        catch (TaskCanceledException ex)
        {
            Console.WriteLine($"Timeout: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient?.Dispose();
        }
    }
}
