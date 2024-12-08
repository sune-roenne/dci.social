using DCI.Social.Identity;
using DCI.Social.Identity.Middleware;
using DCI.Social.UI.Server;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;

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
        return builder;
    }

    public static WebApplication UseMiddlewarePipeline(this WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseMiddleware<UrlRedirectRewriteMiddleWare>();
        app.UseStaticFiles();
        app.UseIdentityPipeline<App>();
        return app;
    }

}
