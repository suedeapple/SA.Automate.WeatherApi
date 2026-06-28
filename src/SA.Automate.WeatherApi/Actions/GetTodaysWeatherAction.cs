using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SA.Automate.WeatherApi.Configuration;
using SA.Automate.WeatherApi.Connection;
using SA.Automate.WeatherApi.Http;
using Umbraco.Automate.Core.Actions;

namespace SA.Automate.WeatherApi.Actions;

/// <summary>
/// Umbraco Automate action that gets today's weather forecast for a location from
/// WeatherAPI.com, using the connection's API key if one is configured.
/// </summary>
[Action("weatherApi.GetTodaysWeather", "Get Today's Weather",
    Description = "Gets today's weather forecast for a location",
    Group = "Weather",
    Icon = "icon-partly-cloudy",
    ConnectionTypeAlias = "weatherApi")]
public class GetTodaysWeatherAction : ActionBase<GetTodaysWeatherSettings, GetTodaysWeatherOutput>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GetTodaysWeatherAction> _logger;
    private readonly IOptionsMonitor<WeatherApiSettings> _weatherApiSettings;

    public GetTodaysWeatherAction(
        ActionInfrastructure infrastructure,
        IHttpClientFactory httpClientFactory,
        ILogger<GetTodaysWeatherAction> logger,
        IOptionsMonitor<WeatherApiSettings> weatherApiSettings)
        : base(infrastructure)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _weatherApiSettings = weatherApiSettings;
    }

    /// <summary>
    /// Executes the action by requesting today's weather forecast for the configured location
    /// from the WeatherAPI.com forecast endpoint. Returns a failed result on validation
    /// errors, missing configuration, or a non-success API response.
    /// </summary>
    public override async Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<GetTodaysWeatherSettings>();

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

        var culture = WeatherApiRequestHelper.ResolveCulture(settings.Culture, _weatherApiSettings.CurrentValue.Culture);

        string? languageCode = null;
        if (!string.IsNullOrWhiteSpace(culture))
        {
            try
            {
                languageCode = WeatherApiRequestHelper.ResolveLanguageCode(CultureInfo.GetCultureInfo(culture));
            }
            catch (CultureNotFoundException ex)
            {
                return ActionResult.Failed(
                    new ArgumentException($"\"{culture}\" is not a recognized culture.", ex),
                    StepRunErrorCategory.Validation);
            }
        }

        try
        {
            var httpClient = _httpClientFactory.CreateClient();

            var result = await WeatherApiRequestHelper.GetTodaysWeatherAsync(
                httpClient, apiKey, settings.Location, languageCode, cancellationToken);

            if (!result.IsSuccess)
            {
                _logger.LogError("WeatherAPI.com request failed with status {StatusCode}: {ErrorContent}",
                    result.StatusCode, result.RawBody);

                return ActionResult.Failed(
                    new Exception(result.ErrorMessage ?? $"WeatherAPI.com returned status {result.StatusCode}"),
                    StepRunErrorCategory.InvalidResponse);
            }

            var location = result.Response?.Location;
            var forecastDay = result.Response?.Forecast?.ForecastDay?.FirstOrDefault();
            var day = forecastDay?.Day;
            var condition = day?.Condition;
            var iconUrl = condition?.Icon;
            if (!string.IsNullOrWhiteSpace(iconUrl) && iconUrl.StartsWith("//"))
            {
                iconUrl = $"https:{iconUrl}";
            }

            _logger.LogInformation("Retrieved today's weather for {Location} from WeatherAPI.com", location?.Name);

            return Success(new GetTodaysWeatherOutput
            {
                LocationName = location?.Name,
                Region = location?.Region,
                Country = location?.Country,
                LocalTime = location?.Localtime,
                Date = forecastDay?.Date,
                MaxTemperatureC = day?.MaxTempC ?? 0,
                MaxTemperatureF = day?.MaxTempF ?? 0,
                MinTemperatureC = day?.MinTempC ?? 0,
                MinTemperatureF = day?.MinTempF ?? 0,
                AvgTemperatureC = day?.AvgTempC ?? 0,
                AvgTemperatureF = day?.AvgTempF ?? 0,
                MaxWindMph = day?.MaxWindMph ?? 0,
                MaxWindKph = day?.MaxWindKph ?? 0,
                TotalPrecipMm = day?.TotalPrecipMm ?? 0,
                TotalPrecipIn = day?.TotalPrecipIn ?? 0,
                TotalSnowCm = day?.TotalSnowCm ?? 0,
                AvgVisKm = day?.AvgVisKm ?? 0,
                AvgVisMiles = day?.AvgVisMiles ?? 0,
                AvgHumidity = day?.AvgHumidity ?? 0,
                WillItRain = day?.DailyWillItRain == 1,
                ChanceOfRain = day?.DailyChanceOfRain ?? 0,
                WillItSnow = day?.DailyWillItSnow == 1,
                ChanceOfSnow = day?.DailyChanceOfSnow ?? 0,
                Condition = condition?.Text,
                ConditionIconUrl = iconUrl,
                ConditionCode = condition?.Code ?? 0,
                Uv = day?.Uv ?? 0,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get today's weather from WeatherAPI.com");
            return ActionResult.Failed(ex, StepRunErrorCategory.InvalidResponse);
        }
    }
}
