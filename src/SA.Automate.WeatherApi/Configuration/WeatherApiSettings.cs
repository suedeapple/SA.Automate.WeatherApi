namespace SA.Automate.WeatherApi.Configuration;

/// <summary>
/// Global WeatherAPI.com settings bound from the <c>appsettings.json</c> configuration section
/// <c>Umbraco:Automate:Providers:SA.Automate.WeatherApi</c>.
/// </summary>
public class WeatherApiSettings
{
    /// <summary>
    /// The configuration section path used to bind these settings.
    /// </summary>
    public const string SectionName = "Umbraco:Automate:Providers:SA.Automate.WeatherApi";

    /// <summary>
    /// The default WeatherAPI.com API key used by connections, unless a connection overrides it
    /// with its own API Key.
    /// </summary>
    public string? ApiKey { get; set; }
}
