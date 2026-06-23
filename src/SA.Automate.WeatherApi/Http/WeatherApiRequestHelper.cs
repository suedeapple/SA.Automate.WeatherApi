using System.Net;
using System.Text.Json;
using SA.Automate.WeatherApi.Models;
using Umbraco.Automate.Core.Connections;

namespace SA.Automate.WeatherApi.Http;

/// <summary>
/// Shared request-building logic for talking to the WeatherAPI.com API, used by both the
/// connection type and the action so the endpoint and key resolution only need to be defined once.
/// </summary>
internal static class WeatherApiRequestHelper
{
    private const string BaseUrl = "https://api.weatherapi.com/v1";

    /// <summary>
    /// A well-known location used to verify an API key without requiring the caller to supply one.
    /// </summary>
    private const string TestLocation = "London";

    /// <summary>
    /// Resolves the effective API key: a per-connection override wins, otherwise the globally
    /// configured default. Every WeatherAPI.com request needs a key, so the default is always
    /// used when no override is set.
    /// </summary>
    public static string? ResolveApiKey(string? connectionOverride, string? globalDefault)
    {
        return !string.IsNullOrWhiteSpace(connectionOverride) ? connectionOverride : globalDefault;
    }

    /// <summary>
    /// Requests the current weather conditions for a location from the WeatherAPI.com
    /// <c>current.json</c> endpoint.
    /// </summary>
    public static async Task<WeatherApiResult> GetCurrentWeatherAsync(
        HttpClient client,
        string apiKey,
        string location,
        CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/current.json?key={Uri.EscapeDataString(apiKey)}&q={Uri.EscapeDataString(location)}";

        using var response = await client.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new WeatherApiResult(false, null, TryParseErrorMessage(body), body, response.StatusCode);
        }

        var parsed = JsonSerializer.Deserialize<WeatherApiResponse>(body);
        return new WeatherApiResult(true, parsed, null, body, response.StatusCode);
    }

    /// <summary>
    /// Validates an API key with a lightweight current-weather request against a well-known
    /// location, used to power the back office "Test connection" action.
    /// </summary>
    public static async Task<ConnectionValidationResult> ValidateApiKeyAsync(
        HttpClient client,
        string apiKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await GetCurrentWeatherAsync(client, apiKey, TestLocation, cancellationToken);

            return result.IsSuccess
                ? ConnectionValidationResult.Success("Connected")
                : ConnectionValidationResult.Failure(
                    result.ErrorMessage ?? $"WeatherAPI.com rejected the request with status {(int)result.StatusCode} ({result.StatusCode}).");
        }
        catch (Exception ex)
        {
            return ConnectionValidationResult.Failure("Could not reach the WeatherAPI.com server.", [ex.Message]);
        }
    }

    private static string? TryParseErrorMessage(string body)
    {
        try
        {
            return JsonSerializer.Deserialize<WeatherApiErrorResponse>(body)?.Error?.Message;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>
/// The outcome of a WeatherAPI.com current weather request.
/// </summary>
internal sealed record WeatherApiResult(bool IsSuccess, WeatherApiResponse? Response, string? ErrorMessage, string RawBody, HttpStatusCode StatusCode);
