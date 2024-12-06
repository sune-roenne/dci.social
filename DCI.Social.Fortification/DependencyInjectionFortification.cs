using DCI.Social.Fortification.Authentication;
using DCI.Social.Fortification.Configuration;
using DCI.Social.Fortification.Encryption;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Fortification;
public static class DependencyInjectionFortification
{
    public static WebApplicationBuilder AddFortificationConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<FortificationConfiguration>(builder.Configuration.GetSection(FortificationConfiguration.ConfigurationElementName));
        return builder;
    }

    public static WebApplicationBuilder AddFortificationEncryption(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFortificationEncryptionService, FortificationEncryptionService>();
        return builder;
    }

    public static AuthenticationBuilder AddFOBFortificationHQAuthentication(this AuthenticationBuilder builder, IConfiguration applicationConfiguration)
    {
        builder
            .AddScheme<FortificationAuthenticationOptions, FortificationAuthenticationHandler>(FortificationAuthenticationConstants.AuthenticationType, opts =>
            {
                var conf = new FortificationConfiguration();
                applicationConfiguration.Bind(FortificationConfiguration.ConfigurationElementName, conf);
                opts.FortificationConfiguration = conf;
            });
        return builder;
    }




}
