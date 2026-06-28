using Microsoft.Extensions.DependencyInjection;
using SA.Automate.WeatherApi.Actions;
using SA.Automate.WeatherApi.Connection;
using Umbraco.Automate.Core.Actions;
using Umbraco.Automate.Core.Connections;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SA.Automate.WeatherApi.Configuration;

/// <summary>
/// Registers all WeatherAPI.com Automate services with the Umbraco dependency injection container.
/// This composer wires up the global settings, connection types, and available actions.
/// </summary>
public class WeatherApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Bind the WeatherAPI.com settings section from appsettings.json (e.g. ApiKey)
        builder.Services.AddOptions<WeatherApiSettings>()
            .BindConfiguration(WeatherApiSettings.SectionName);

        // Register the WeatherAPI.com connection type so it appears in Umbraco Automate connections
        builder.WithCollectionBuilder<ConnectionTypeCollectionBuilder>()
            .Add<WeatherApiConnectionType>();

        // Register the Get Current Weather and Get Today's Weather actions so they are available
        // in Umbraco Automate workflows
        builder.WithCollectionBuilder<ActionCollectionBuilder>()
            .Add<GetCurrentWeatherAction>()
            .Add<GetTodaysWeatherAction>();
    }
}
