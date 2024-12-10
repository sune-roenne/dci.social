using DCI.Social.FOB.Central;
using DCI.Social.FOB.Contest;
using DCI.Social.FOB.HeadQuarters;
using DCI.Social.Fortification;
using DCI.Social.Messages.Locations;
using Microsoft.AspNetCore.Builder;

namespace DCI.Social.FOB;

public static class DependencyInjection
{

    public static WebApplicationBuilder AddFOBConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.AddFortificationConfiguration();
        return builder;
    }

    public static WebApplicationBuilder AddFOBServices(this WebApplicationBuilder builder)
    {
        builder.AddFrameworkServices();
        builder.AddSecurityServices();
        builder.Services.AddSingleton<IFOBControlService, FOBControlService>();
        return builder;
    }

    private static void AddFrameworkServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSignalR(opts =>
        {

        });
    }

    private static WebApplicationBuilder AddSecurityServices(this WebApplicationBuilder builder)
    {
        builder.AddFortificationEncryption();
        builder.Services.AddAuthentication()
            .AddFOBFortificationHQAuthentication(builder.Configuration);

        return builder;
    }

    public static WebApplication UseFOBRequestPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseAuthentication();
        app.MapControllers();
        app.MapHub<ContestHub>("/client/contest", opts =>
        {
            
        });
        app.MapHub<HeadQuartersHub>($"/{FOBLocations.HeadQuartersHub}", opts =>
        {
        });

        return app;
    }



}
