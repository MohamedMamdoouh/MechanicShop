using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MechanicShop.Application.Features.Identity;
using MechanicShop.Contracts.Identity;
namespace MechanicShop.Api.IntegrationTests.Common;

public sealed class AppHttpClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient = httpClient;

    public async Task AuthenticateAsync(string email, string password, string device = "test-device")
    {
        var loginRequest = new LoginRequest(email, password, device);
        var response = await _httpClient.PostAsJsonAsync("/api/v1/identity/login", loginRequest, JsonOptions);

        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(JsonOptions)
            ?? throw new InvalidOperationException("Login response did not contain a valid token.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
    }

    public void ClearAuthentication() =>
        _httpClient.DefaultRequestHeaders.Authorization = null;

    public Task<HttpResponseMessage> GetAsync(string url) =>
        _httpClient.GetAsync(url);

    public async Task<(HttpResponseMessage Response, T? Body)> GetAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return (response, default);
        var body = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
        return (response, body);
    }

    public Task<HttpResponseMessage> PostAsync<TRequest>(string url, TRequest body) =>
        _httpClient.PostAsJsonAsync(url, body, JsonOptions);

    public async Task<(HttpResponseMessage Response, TResponse? Body)> PostAsync<TRequest, TResponse>(
        string url, TRequest body)
    {
        var response = await _httpClient.PostAsJsonAsync(url, body, JsonOptions);
        if (!response.IsSuccessStatusCode) return (response, default);
        var responseBody = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions);
        return (response, responseBody);
    }

    public Task<HttpResponseMessage> PutAsync<TRequest>(string url, TRequest body) =>
        _httpClient.PutAsJsonAsync(url, body, JsonOptions);

    public Task<HttpResponseMessage> DeleteAsync(string url) =>
        _httpClient.DeleteAsync(url);
}
