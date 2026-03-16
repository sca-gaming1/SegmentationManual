using System.Text.Json;
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
        
        // Increase timeout for slow APIs (like 777)
        // Default is 100 seconds, we increase to 10 minutes
        _httpClient.Timeout = TimeSpan.FromMinutes(10);
    }

    public async Task<bool> HideFieldAsync(
        string dataSourceId,
        string fieldName,
        HttpMethod method,
        CancellationToken cancellationToken = default
    )
    {
        var endpoint = $"/api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Hide";
        return await ExecuteVisibilityRequestAsync(endpoint, method, cancellationToken);
    }

    public async Task<bool> UnhideFieldAsync(
        string dataSourceId,
        string fieldName,
        HttpMethod method,
        CancellationToken cancellationToken = default
    )
    {
        var endpoint = $"/api/DataSources/{dataSourceId}/Fields/{fieldName}/Visibility/Unhide";
        return await ExecuteVisibilityRequestAsync(endpoint, method, cancellationToken);
    }

    private async Task<bool> ExecuteVisibilityRequestAsync(
        string endpoint,
        HttpMethod method,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var url = $"{_baseUrl}{endpoint}";
            Console.WriteLine($"Executing {method.Method}: {url}");

            var request = new HttpRequestMessage(method, url);

            if (method == HttpMethod.Post || method == HttpMethod.Put)
            {
                request.Content = new StringContent(
                    "{}",
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
            }

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

    public async Task<List<Segment>> GetSegmentsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"{_baseUrl}/api/Segments";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


                try
                {
                    var segments = JsonSerializer.Deserialize<List<Segment>>(content, options);
                    if (segments != null)
                    {
                        return segments;
                    }
                }
                catch
                {
                    // If that fails, try to deserialize as a wrapped response
                    try
                    {
                        using var doc = JsonDocument.Parse(content);
                        var root = doc.RootElement;

                        // Check common wrapper property names
                        JsonElement dataElement;
                        if (root.TryGetProperty("data", out dataElement) ||
                            root.TryGetProperty("segments", out dataElement) ||
                            root.TryGetProperty("items", out dataElement) ||
                            root.TryGetProperty("results", out dataElement))
                        {
                            var segments = JsonSerializer.Deserialize<List<Segment>>(
                                dataElement.GetRawText(),
                                options
                            );
                            return segments ?? new List<Segment>();
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Console.WriteLine($"Error parsing wrapped response: {innerEx.Message}");
                    }
                }

                Console.WriteLine("Warning: Could not parse segments from response");
                return new List<Segment>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"Error {response.StatusCode}: {errorContent}");
                return new List<Segment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching segments: {ex.Message}");
            return new List<Segment>();
        }
    }

    public async Task<(SegmentDetail? detail, string rawJson)> GetSegmentDetailAsync(
        string segmentId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var url = $"{_baseUrl}/api/InHouseSegments/{segmentId}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                // We don't need the deserialized object, just return the raw JSON
                // Ignore deserialization errors since we only use the raw JSON
                return (null, content);
            }
            else
            {
                // Don't log errors here, let the caller handle them
                return (null, string.Empty);
            }
        }
        catch (TaskCanceledException)
        {
            // Timeout - don't log, let caller retry
            throw;
        }
        catch (HttpRequestException)
        {
            // HTTP error - don't log, let caller retry
            throw;
        }
        catch (Exception)
        {
            // Other errors - don't log, let caller retry
            throw;
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
