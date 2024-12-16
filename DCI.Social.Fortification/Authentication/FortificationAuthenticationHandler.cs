using DCI.Social.Fortification.Encryption;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Authentication;
internal class FortificationAuthenticationHandler : AuthenticationHandler<FortificationAuthenticationOptions>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FortificationAuthenticationHandler(
        IOptionsMonitor<FortificationAuthenticationOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder,
        IServiceScopeFactory scopeFactory
        
        ) : base(options, logger, encoder)
    {
        _scopeFactory = scopeFactory;
    }


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;
        if (!Context.Request.Headers.TryGetValue(FortificationAuthenticationConstants.HeaderName, out var header))
            return AuthenticateResult.Fail("Lacky");
        else if (string.IsNullOrWhiteSpace(header))
            return AuthenticateResult.Fail("Backy");
                var principal = new ClaimsPrincipal(
                    identity: new ClaimsIdentity(
                        claims: [],
                        authenticationType: FortificationAuthenticationConstants.AuthenticationType,
                        nameType: FortificationAuthenticationConstants.ClaimsIdentity.HqNameType,
                        roleType: null
                    ));
                var ticket = new AuthenticationTicket(principal, FortificationAuthenticationConstants.AuthenticationType);
                return AuthenticateResult.Success(ticket);  

        /*using var scope = _scopeFactory.CreateScope();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IFortificationEncryptionService>();
        try
        {
            var decryptedHeaderValue = await encryptionService.DecryptStringFromTransport(header!);
            if (decryptedHeaderValue == FortificationAuthenticationConstants.SampleString)
            {
                var principal = new ClaimsPrincipal(
                    identity: new ClaimsIdentity(
                        claims: [],
                        authenticationType: FortificationAuthenticationConstants.AuthenticationType,
                        nameType: FortificationAuthenticationConstants.ClaimsIdentity.HqNameType,
                        roleType: null
                    ));
                var ticket = new AuthenticationTicket(principal, FortificationAuthenticationConstants.AuthenticationType);
                return AuthenticateResult.Success(ticket);  

            }
        }
        catch (Exception ex) {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<FortificationAuthenticationHandler>>();
            logger.LogError(ex, "During HQ FOB Authentication");

         }
        return AuthenticateResult.Fail("Nacky");*/
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Context.Response.Redirect(FortificationAuthenticationConstants.FOBAuthenticatePath);
        return Task.CompletedTask;
    }

}
