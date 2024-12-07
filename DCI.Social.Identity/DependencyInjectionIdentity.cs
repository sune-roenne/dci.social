using Microsoft.AspNetCore.Authentication.Cookies;
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
        return builder;
            
    }


    public static WebApplication UseIdentityPipeline<TApp>(this WebApplication app) where TApp : class
    {
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseAntiforgery();
        app.MapRazorComponents<TApp>()
            .AddInteractiveServerRenderMode();
        app.MapControllers();
        return app;
    }


    internal static Action<AuthorizationPolicyBuilder> IsAuthenticatedPolicy = builder =>
    {
        builder.RequireAuthenticatedUser();
    };

}
