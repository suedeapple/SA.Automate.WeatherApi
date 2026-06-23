using Microsoft.Extensions.Options;
using SA.Automate.WeatherApi.Configuration;
using SA.Automate.WeatherApi.Http;
using Umbraco.Automate.Core.Connections;

namespace SA.Automate.WeatherApi.Connection;

/// <summary>
/// Defines the WeatherAPI.com connection type for Umbraco Automate.
/// Stores an optional access key override per connection, and validates it against the
/// WeatherAPI.com current weather endpoint before saving.
/// </summary>
[ConnectionType("weatherApi", "WeatherAPI.com",
    Description = "Connect to WeatherAPI.com using an API key",
    Group = "Weather",
    Icon = "icon-plugin")]
public sealed class WeatherApiConnectionType : ConnectionTypeBase<WeatherApiConnectionSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<WeatherApiSettings> _weatherApiSettings;

    public WeatherApiConnectionType(
        ConnectionTypeInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<WeatherApiSettings> weatherApiSettings)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _weatherApiSettings = weatherApiSettings;
    }

    /// <summary>
    /// Validates the connection by requesting the current weather for a known location
    /// using the resolved API key.
    /// </summary>
    public override async Task<ConnectionValidationResult> ValidateAsync(
        object? settings,
        CancellationToken cancellationToken)
    {
        if (settings is not WeatherApiConnectionSettings typed)
            return ConnectionValidationResult.Failure("Connection settings are missing.");

        var apiKey = WeatherApiRequestHelper.ResolveApiKey(typed.ApiKey, _weatherApiSettings.CurrentValue.ApiKey);

        if (string.IsNullOrWhiteSpace(apiKey))
            return ConnectionValidationResult.Failure("An API Key is required, either on the connection or in appsettings.json.");

        using var client = _httpClientFactory.CreateClient();

        return await WeatherApiRequestHelper.ValidateApiKeyAsync(client, apiKey, cancellationToken);
    }
}
