using DCI.Social.HeadQuarters.Configuration;
using DCI.Social.HeadQuarters.FOB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DCI.Social.HeadQuarters;
public static class HeadQuartersDependencyInjection
{

    public static WebApplicationBuilder AddHeadQuartersConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<HeadQuartersConfiguration>(builder.Configuration.GetSection(HeadQuartersConfiguration.ConfigurationElementName));
        return builder;
    }


    public static WebApplicationBuilder AddHeadQuarters(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IFOBService, FOBService>();
        return builder;
    }


}
