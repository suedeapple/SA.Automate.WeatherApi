using Umbraco.Automate.Core.Settings;

namespace SA.Automate.WeatherApi.Connection;

/// <summary>
/// Stores the settings for a WeatherAPI.com connection in Umbraco Automate.
/// </summary>
public sealed class WeatherApiConnectionSettings
{
    /// <summary>
    /// Optionally overrides the globally configured WeatherAPI.com API key for this connection.
    /// Leave blank to use the default key (Umbraco:Automate:Providers:SA.Automate.WeatherApi:ApiKey).
    /// </summary>
    [Field(
        Label = "API Key",
        Description = "Optional. Overrides the globally configured WeatherAPI.com API key for this connection. Leave blank to use the default configured in appsettings.json.",
        IsSensitive = true,
        SortOrder = 1)]
    public string? ApiKey { get; set; }
}
