using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DurableAzureFuncBoilerplate.Upstream.Clients;
using DurableAzureFuncBoilerplate.Configuration;

[assembly: FunctionsStartup(typeof(DurableAzureFuncBoilerplate.Startup))]

namespace DurableAzureFuncBoilerplate;
    
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddSingleton<IBoredClient, BoredClient>();
        
        builder.Services.AddOptions<BoredClientConfig>()
            .Configure<IConfiguration>((boredClientConfig, configuration) =>
            {
                configuration.GetSection(BoredClientConfig.Section).Bind(boredClientConfig);
            });
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        FunctionsHostBuilderContext context = builder.GetContext();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: false, reloadOnChange: false)
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"appsettings.{context.EnvironmentName}.json"), optional: true, reloadOnChange: false)
            .AddEnvironmentVariables();
    }
}