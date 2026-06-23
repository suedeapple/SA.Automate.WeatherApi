using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SA.Automate.WeatherApi.Configuration;
using SA.Automate.WeatherApi.Connection;
using SA.Automate.WeatherApi.Http;
using Umbraco.Automate.Core.Actions;

namespace SA.Automate.WeatherApi.Actions;

/// <summary>
/// Umbraco Automate action that gets the current weather conditions for a location from
/// WeatherAPI.com, using the connection's API key if one is configured.
/// </summary>
[Action("weatherApi.GetCurrentWeather", "Get Current Weather",
    Description = "Gets the current weather conditions for a location",
    Group = "Weather",
    Icon = "icon-partly-cloudy",
    ConnectionTypeAlias = "weatherApi")]
public class GetCurrentWeatherAction : ActionBase<GetCurrentWeatherSettings, GetCurrentWeatherOutput>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetCurrentWeatherAction> _logger;
    private readonly IOptionsMonitor<WeatherApiSettings> _weatherApiSettings;

    public GetCurrentWeatherAction(
        ActionInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        ILogger<GetCurrentWeatherAction> logger,
        IOptionsMonitor<WeatherApiSettings> weatherApiSettings)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _weatherApiSettings = weatherApiSettings;
    }

    /// <summary>
    /// Executes the action by requesting the current weather for the configured location
    /// from the WeatherAPI.com current weather endpoint. Returns a failed result on validation
    /// errors, missing configuration, or a non-success API response.
    /// </summary>
    public override async Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<GetCurrentWeatherSettings>();

        if (string.IsNullOrWhiteSpace(settings.Location))
        {
            return ActionResult.Failed(
                new ArgumentException("Location is required."),
                StepRunErrorCategory.Validation);
        }

        var connectionSettings = context.Connection?.GetSettings<WeatherApiConnectionSettings>();
        var apiKey = WeatherApiRequestHelper.ResolveApiKey(connectionSettings?.ApiKey, _weatherApiSettings.CurrentValue.ApiKey);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return ActionResult.Failed(
                new ArgumentException("A WeatherAPI.com API Key is not configured, either on the connection or in appsettings.json."),
                StepRunErrorCategory.Validation);
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var result = await WeatherApiRequestHelper.GetCurrentWeatherAsync(
                httpClient, apiKey, settings.Location, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError("WeatherAPI.com request failed with status {StatusCode}: {ErrorContent}",
                    result.StatusCode, result.RawBody);

                return ActionResult.Failed(
                    new Exception(result.ErrorMessage ?? $"WeatherAPI.com returned status {result.StatusCode}"),
                    StepRunErrorCategory.InvalidResponse);
            }

            var location = result.Response?.Location;
            var current = result.Response?.Current;
            var condition = current?.Condition;
            var iconUrl = condition?.Icon;
            if (!string.IsNullOrWhiteSpace(iconUrl) && iconUrl.StartsWith("//"))
            {
                iconUrl = $"https:{iconUrl}";
            }

            _logger.LogInformation("Retrieved current weather for {Location} from WeatherAPI.com", location?.Name);

            return Success(new GetCurrentWeatherOutput
            {
                LocationName = location?.Name,
                Region = location?.Region,
                Country = location?.Country,
                LocalTime = location?.Localtime,
                TemperatureC = current?.TempC ?? 0,
                TemperatureF = current?.TempF ?? 0,
                Condition = condition?.Text,
                ConditionIconUrl = iconUrl,
                ConditionCode = condition?.Code ?? 0,
                Humidity = current?.Humidity ?? 0,
                Cloud = current?.Cloud ?? 0,
                WindKph = current?.WindKph ?? 0,
                WindMph = current?.WindMph ?? 0,
                WindDirection = current?.WindDir,
                LastUpdated = current?.LastUpdated,
                WillItRain = current?.WillItRain == 1,
                ChanceOfRain = current?.ChanceOfRain ?? 0,
                WillItSnow = current?.WillItSnow == 1,
                ChanceOfSnow = current?.ChanceOfSnow ?? 0,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current weather from WeatherAPI.com");
            return ActionResult.Failed(ex, StepRunErrorCategory.InvalidResponse);
        }
    }
}
