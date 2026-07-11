using Umbraco.Automate.Core.Settings;

namespace SA.Automate.WeatherApi.Connection;

/// <summary>
/// Stores the settings for a WeatherAPI.com connection in Umbraco Automate.
/// </summary>
public sealed class WeatherApiConnectionSettings
{
    /// <summary>
    /// The WeatherAPI.com API key used by this connection.
    /// </summary>
    [Field(
        Label = "API Key",
        Description = "The WeatherAPI.com API key used by this connection.",
        IsSensitive = true,
        SortOrder = 1)]
    public string ApiKey { get; set; } = "$Umbraco:Automate:Secrets:WeatherApiKey";
}
