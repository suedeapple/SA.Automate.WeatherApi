# SA.Automate.WeatherApi

[![Downloads](https://img.shields.io/nuget/dt/SA.Automate.WeatherApi?color=cc9900)](https://www.nuget.org/packages/SA.Automate.WeatherApi/)
[![NuGet](https://img.shields.io/nuget/vpre/SA.Automate.WeatherApi?color=0273B3)](https://www.nuget.org/packages/SA.Automate.WeatherApi)
[![GitHub license](https://img.shields.io/github/license/suedeapple/SA.Automate.WeatherApi?color=8AB803)](https://github.com/suedeapple/SA.Automate.WeatherApi/blob/main/LICENSE)

WeatherAPI.com connection types and actions for [Umbraco Automate](https://github.com/umbraco/Umbraco.Automate). Get the current weather for a location as part of an automation workflow.

## What is WeatherAPI.com?

[WeatherAPI.com](https://www.weatherapi.com/) is a JSON/XML weather API that provides current weather, forecasts, and historical weather for locations worldwide, looked up by city name, postcode/zip, IP address, or latitude/longitude.

## What can this be used for?

This package is useful when you want current weather data inside an Umbraco Automate workflow, for example:

- **Conditional content**: branch a workflow based on the current conditions at a location.
- **Enriching notifications**: include the current temperature or conditions in a message sent by another action.
- **Logging/reporting**: record weather conditions alongside other workflow data.

## Installation

```bash
dotnet add package SA.Automate.WeatherApi
```

No further setup required. The composer registers itself automatically via Umbraco's `IComposer` discovery.

## Connection types

This package registers a single **WeatherAPI.com** connection type:

- **API Key**: optional. Overrides the globally configured API key for this connection. Leave blank to use the default key configured in `appsettings.json`.

## Setup

### 1. Configure your API key

Get a free API key from [weatherapi.com](https://www.weatherapi.com/) and set it as the default in your `appsettings.json` (or `appsettings.Production.json`); individual connections can still override it with their own key:

```json
{
  "Umbraco": {
    "Automate": {
      "Providers": {
        "SA.Automate.WeatherApi": {
          "ApiKey": "your-api-key",
          "DefaultCulture": "en-GB"
        }
      }
    }
  }
}
```

`DefaultCulture` is optional and sets the default culture (e.g. `en-GB`, `fr-FR`) used to localize weather condition text across all actions. Leave it unset to default to English. An action's own **Culture** field always overrides this.

### 2. Create the connection in the backoffice

1. Go to **Automate → Connections** and create a new **WeatherAPI.com** connection.
2. Give the connection a name. Leave **API Key** blank to use the default configured in `appsettings.json`, or enter a different key for this connection.
3. Click **Test connection** to verify. This requests the current weather for a known location to confirm the API key is valid.

**Tip:** You only need multiple connections if different workflows should use different API keys.

## Usage

Add the **Get Current Weather** action to any automation and select the connection to use. Available fields:

| Field | Description |
|---|---|
| Location | The location to get current weather for. Accepts a city name, US zip, UK postcode, IP address, or `lat,lon`. Supports `${ binding }` expressions. |
| Culture | Optional. The culture to localize the weather condition text into, e.g. `en-GB`, `fr-FR`, `es-ES`. Overrides the globally configured `DefaultCulture`. Falls back to that default, or English if neither is set. Supports `${ binding }` expressions. |

The action outputs the following, which can be referenced via bindings in later workflow steps:

| Output | Description |
|---|---|
| LocationName | The resolved location name, e.g. "London". |
| Region | The resolved region. |
| Country | The resolved country. |
| LocalTime | The local time at the resolved location. |
| TemperatureC | The current temperature in degrees Celsius. |
| TemperatureF | The current temperature in degrees Fahrenheit. |
| Condition | The weather condition text, e.g. "Sunny". |
| ConditionIconUrl | The URL of the icon representing the weather condition. |
| ConditionCode | The WeatherAPI.com condition code, e.g. `1000` for "Sunny". |
| Humidity | The current humidity, as a percentage. |
| Cloud | The current cloud cover, as a percentage. |
| WindKph | The current wind speed in kilometres per hour. |
| WindMph | The current wind speed in miles per hour. |
| WindDirection | The current wind direction, e.g. "WSW". |
| LastUpdated | The time the weather data was last updated. |
| WillItRain | Whether it is expected to rain during the current hour. |
| ChanceOfRain | The chance of rain during the current hour, as a percentage. |
| WillItSnow | Whether it is expected to snow during the current hour. |
| ChanceOfSnow | The chance of snow during the current hour, as a percentage. |

## Compatibility

| Package version | Umbraco Automate | Umbraco CMS |
|---|---|---|
| 1.x | 17.x | 17.x |

## Links

- [Source code](https://github.com/suedeapple/SA.Automate.WeatherApi)
- [Report an issue](https://github.com/suedeapple/SA.Automate.WeatherApi/issues)
- [WeatherAPI.com documentation](https://www.weatherapi.com/docs/)
