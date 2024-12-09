using DCI.Social.Identity.Configuration;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;

namespace DCI.Social.Identity;
public static class DependencyInjectionIdentity
{




    public static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration);
        builder.Services.AddAuthorization(opts =>
        {
            opts.AddPolicy(IdentityConstants.IsAuthenticatedPolicyName , IsAuthenticatedPolicy);
        });
        builder.Services.Configure<IdentityConfiguration>(builder.Configuration.GetSection(IdentityConfiguration.ConfigurationElementName));
        builder.Services.Configure<AzureAdConfiguration>(builder.Configuration.GetSection(AzureAdConfiguration.ConfigurationElementName));

        return builder;
    }


    public static WebApplication UseIdentityPipeline<TApp>(this WebApplication app, string? hostingBasePath) where TApp : class
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapRazorComponents<TApp>()
            .AddInteractiveServerRenderMode();
        if(!string.IsNullOrWhiteSpace(hostingBasePath))
            app.MapBlazorHub("/" +  hostingBasePath);
        app.MapControllers();
        return app;
    }


    internal static Action<AuthorizationPolicyBuilder> IsAuthenticatedPolicy = builder =>
    {
        builder.RequireAuthenticatedUser();
    };

}
