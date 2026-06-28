using System.Globalization;
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
    /// Resolves the effective culture: an action-level override wins, otherwise the globally
    /// configured default culture (<c>Culture</c> in appsettings.json). Returns null
    /// when neither is set, in which case WeatherAPI.com falls back to English.
    /// </summary>
    public static string? ResolveCulture(string? actionOverride, string? globalDefault)
    {
        return !string.IsNullOrWhiteSpace(actionOverride) ? actionOverride : globalDefault;
    }

    /// <summary>
    /// Requests the current weather conditions for a location from the WeatherAPI.com
    /// <c>current.json</c> endpoint, optionally localizing the condition text into
    /// <paramref name="languageCode"/> (a WeatherAPI.com <c>lang</c> value, see
    /// <see cref="ResolveLanguageCode"/>).
    /// </summary>
    public static async Task<WeatherApiResult> GetCurrentWeatherAsync(
        HttpClient client,
        string apiKey,
        string location,
        string? languageCode,
        CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/current.json?key={Uri.EscapeDataString(apiKey)}&q={Uri.EscapeDataString(location)}";

        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            url += $"&lang={Uri.EscapeDataString(languageCode)}";
        }

        using var response = await client.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new WeatherApiResult(false, null, TryParseErrorMessage(body), body, response.StatusCode);
        }

        var parsed = JsonSerializer.Deserialize<WeatherApiCurrentResponse>(body);
        return new WeatherApiResult(true, parsed, null, body, response.StatusCode);
    }

    /// <summary>
    /// Requests today's weather forecast for a location from the WeatherAPI.com
    /// <c>forecast.json</c> endpoint (requested with <c>days=1</c>), optionally localizing the
    /// condition text into <paramref name="languageCode"/> (a WeatherAPI.com <c>lang</c> value,
    /// see <see cref="ResolveLanguageCode"/>).
    /// </summary>
    public static async Task<WeatherApiForecastResult> GetTodaysWeatherAsync(
        HttpClient client,
        string apiKey,
        string location,
        string? languageCode,
        CancellationToken cancellationToken)
    {
        var url = $"{BaseUrl}/forecast.json?key={Uri.EscapeDataString(apiKey)}&q={Uri.EscapeDataString(location)}&days=1";

        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            url += $"&lang={Uri.EscapeDataString(languageCode)}";
        }

        using var response = await client.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new WeatherApiForecastResult(false, null, TryParseErrorMessage(body), body, response.StatusCode);
        }

        var parsed = JsonSerializer.Deserialize<WeatherApiForecastResponse>(body);
        return new WeatherApiForecastResult(true, parsed, null, body, response.StatusCode);
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
            var result = await GetCurrentWeatherAsync(client, apiKey, TestLocation, null, cancellationToken);

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

    /// <summary>
    /// Maps a .NET culture (e.g. "fr-FR") to the language code WeatherAPI.com's <c>lang</c>
    /// query parameter expects. WeatherAPI.com only distinguishes Chinese by script rather than
    /// region, so Traditional Chinese cultures map to "zh_tw" and everything else falls back to
    /// the two-letter ISO language name.
    /// </summary>
    private static readonly string[] TraditionalChineseMarkers = ["Hant", "TW", "HK", "MO"];

    public static string ResolveLanguageCode(CultureInfo culture)
    {
        if (string.Equals(culture.TwoLetterISOLanguageName, "zh", StringComparison.OrdinalIgnoreCase) &&
            TraditionalChineseMarkers.Any(marker => culture.Name.Contains(marker, StringComparison.OrdinalIgnoreCase)))
        {
            return "zh_tw";
        }

        return culture.TwoLetterISOLanguageName;
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
internal sealed record WeatherApiResult(bool IsSuccess, WeatherApiCurrentResponse? Response, string? ErrorMessage, string RawBody, HttpStatusCode StatusCode);

/// <summary>
/// The outcome of a WeatherAPI.com forecast request.
/// </summary>
internal sealed record WeatherApiForecastResult(bool IsSuccess, WeatherApiForecastResponse? Response, string? ErrorMessage, string RawBody, HttpStatusCode StatusCode);
