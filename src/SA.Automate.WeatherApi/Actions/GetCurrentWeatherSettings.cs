using Umbraco.Automate.Core.Settings;

namespace SA.Automate.WeatherApi.Actions;

/// <summary>
/// Defines the configurable settings for the Get Current Weather action in Umbraco Automate.
/// </summary>
public class GetCurrentWeatherSettings
{
    /// <summary>The location to get current weather for. Required.</summary>
    [Field(
        Label = "Location",
        Description = "The location to get current weather for. Accepts a city name, US zip, UK postcode, IP address, or \"lat,lon\". Supports bindings.",
        SupportsBindings = true)]
    public string Location { get; set; } = string.Empty;
}
