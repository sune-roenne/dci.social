using DCI.Social.Identity;
using DCI.Social.UI.Middleware;
using DCI.Social.UI.Configuration;
using DCI.Social.UI.Server;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using DCI.Social.Fortification;
using DCI.Social.UI.FOB;

namespace DCI.Social.UI;

public static class DependencyInjectionUI
{

    public static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json", optional: false);
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.Configure<UIConfiguration>(builder.Configuration.GetSection(UIConfiguration.ConfigurationElementName));
        builder.AddFortificationConfiguration();
        return builder;
    }


    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {

        builder.AddIdentity();
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddMicrosoftIdentityConsentHandler();
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();
        builder.Services.Configure<ForwardedHeadersOptions>(opts =>
        {
            opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            opts.KnownNetworks.Clear();
            opts.KnownProxies.Clear();
        });
        builder.Services.AddSingleton<IContestService, ContestService>();
        return builder;
    }

    public static WebApplication UseMiddlewarePipeline(this WebApplication app)
    {
        var conf = app.Configuration.UIConfig();
        app.UseMiddleware<UrlRedirectRewriteMiddleWare>();
        app.UseForwardedHeaders();
        app.UseStaticFiles();
        app.UseIdentityPipeline<App>(conf.HostingBasePath);
        return app;
    }

    private static UIConfiguration UIConfig(this IConfiguration conf)
    {
        var returnee = new UIConfiguration();
        conf.Bind(UIConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }

}
