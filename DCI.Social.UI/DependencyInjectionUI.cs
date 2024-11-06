using DCI.Social.Identity;
using DCI.Social.UI.Components;
using DCI.Social.UI.Server;
using NYK.Identity;
using NYK.Identity.Configuration;
using NYK.Identity.UI;

namespace DCI.Social.UI;

public static class DependencyInjectionUI
{

    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", optional: false);
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        return builder;
    }


    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.AddIdentity();
        return builder;
    }

    public static WebApplication UseMiddlewarePipeline(this WebApplication app)
    {
        var conf = app.Services.GetRequiredService<NykreditIdentityConfiguration>();
        app.UseNykreditOpenApiUIWithSecurity<App>();
        return app;

    }

}
