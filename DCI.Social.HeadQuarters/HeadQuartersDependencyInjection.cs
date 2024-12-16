using DCI.Social.HeadQuarters.Configuration;
using DCI.Social.HeadQuarters.Persistance;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        builder.Services.AddSingleton<IHeadQuartersContestAdminRepo, HeadQuartersContestAdminRepo>();
        builder.Services.AddSingleton<IHeadQuartersService, HeadQuartersService>();
        builder.AddHeadQuartersPersistence();

        return builder;
    }


    public static WebApplicationBuilder AddHeadQuartersPersistence(this WebApplicationBuilder builder)
    {
        var hqConf = builder.Configuration.HeadQuartersConfig();
        builder.Services.AddDbContextFactory<SocialDbContext>(opts =>  ConfigureDbContextOptions(opts, hqConf));
        return builder;
    }

    private static void ConfigureDbContextOptions(DbContextOptionsBuilder options, HeadQuartersConfiguration conf)
    {

        options
            .UseOracle(conf.Db.ConnectionString)
            .EnableDetailedErrors();
    }
    private static HeadQuartersConfiguration HeadQuartersConfig(this IConfiguration conf)
    {
        var returnee = new HeadQuartersConfiguration();
        conf.Bind(HeadQuartersConfiguration.ConfigurationElementName,returnee);
        return returnee;
    }


}
