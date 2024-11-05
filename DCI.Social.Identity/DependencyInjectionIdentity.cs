using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using NYK.Identity.Configuration;
using NYK.Identity.UI;

namespace DCI.Social.Identity;
public static class DependencyInjectionIdentity
{

    public static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        var identConfig = NykreditIdentityConfigurationBuilder.From()
            .WithCustomAuthorizationPolicy(IdentityConstants.IsAuthenticatedPolicyName, IsAuthenticatedPolicy)
            .Build();

        builder.AddDefaultNykreditIdentitySetupForUiApp(identityConfigurationOverrides: identConfig);
        return builder;
    }


    internal static Action<AuthorizationPolicyBuilder> IsAuthenticatedPolicy = builder =>
    {
        builder.RequireAuthenticatedUser();
    };

}
