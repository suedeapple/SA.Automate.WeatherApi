using SA.Automate.WeatherApi.Http;
using Umbraco.Automate.Core.Connections;

namespace SA.Automate.WeatherApi.Connection;

/// <summary>
/// Defines the WeatherAPI.com connection type for Umbraco Automate.
/// Stores the API key per connection, and validates it against the
/// WeatherAPI.com current weather endpoint before saving.
/// </summary>
[ConnectionType("weatherApi", "WeatherAPI.com",
    Description = "Connect to WeatherAPI.com using an API key",
    Group = "Weather",
    Icon = "icon-plugin")]
public sealed class WeatherApiConnectionType : ConnectionTypeBase<WeatherApiConnectionSettings>
{
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherApiConnectionType(
        ConnectionTypeInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Validates the connection by requesting the current weather for a known location
    /// using the configured API key.
    /// </summary>
    public override async Task<ConnectionValidationResult> ValidateAsync(
        object? settings,
        CancellationToken cancellationToken)
    {
        if (settings is not WeatherApiConnectionSettings typed)
            return ConnectionValidationResult.Failure("Connection settings are missing.");

        if (string.IsNullOrWhiteSpace(typed.ApiKey))
            return ConnectionValidationResult.Failure("An API Key is required.");

        using var client = _httpClientFactory.CreateClient();

        return await WeatherApiRequestHelper.ValidateApiKeyAsync(client, typed.ApiKey, cancellationToken);
    }
}
