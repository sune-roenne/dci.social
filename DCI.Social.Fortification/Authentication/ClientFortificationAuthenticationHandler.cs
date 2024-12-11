﻿using DCI.Social.Fortification.Encryption;
using DCI.Social.Fortification.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DCI.Social.Fortification.Authentication;
internal class ClientFortificationAuthenticationHandler : AuthenticationHandler<FortificationAuthenticationOptions>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RSA _clientPublicKey;
    private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.Pkcs1;

    public ClientFortificationAuthenticationHandler(
        IOptionsMonitor<FortificationAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IServiceScopeFactory scopeFactory
        ) : base(options, logger, encoder)
    {
        _scopeFactory = scopeFactory;
        var publicKeyString = options.CurrentValue.FortificationConfiguration.ClientCertificateFile!.ReadCertificateFile();
        var certBytes = Convert.FromBase64String(publicKeyString);
        var cert = new X509Certificate2(certBytes);
        _clientPublicKey = cert.GetRSAPublicKey()!;
    }


    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;
        if (!Context.Request.Headers.TryGetValue(FortificationAuthenticationConstants.HeaderName, out var header))
            return AuthenticateResult.Fail("Lacky");
        else if (string.IsNullOrWhiteSpace(header))
            return AuthenticateResult.Fail("Backy");
        try
        {
            var headerValueBytes = Convert.FromBase64String(header!);
            var decryptedHeaderBytes = _clientPublicKey.Decrypt(headerValueBytes, Padding);
            var decryptedHeaderValue = UTF8Encoding.UTF8.GetString(decryptedHeaderBytes);
            if (decryptedHeaderValue == FortificationAuthenticationConstants.SampleString)
            {
                var principal = new ClaimsPrincipal(
                    identity: new ClaimsIdentity(
                        claims: [],
                        authenticationType: FortificationAuthenticationConstants.ClientAuthenticationType,
                        nameType: FortificationAuthenticationConstants.ClaimsIdentity.HqNameType,
                        roleType: null
                    ));
                var ticket = new AuthenticationTicket(principal, FortificationAuthenticationConstants.ClientAuthenticationType);
                return AuthenticateResult.Success(ticket);

            }
        }
        catch (Exception) { }
        return AuthenticateResult.Fail("Nacky");
    }

}
