using DCI.Social.FOB.Central;
using DCI.Social.FOB.Contest;
using Microsoft.AspNetCore.Builder;

namespace DCI.Social.FOB;

public static class DependencyInjection
{

    public static WebApplicationBuilder AddFOBConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile("appsettings.local.json", optional: true);
        builder.Configuration.AddEnvironmentVariables();
        return builder;
    }

    public static WebApplicationBuilder AddFOBServices(this WebApplicationBuilder builder)
    {
        builder.AddFrameworkServices();
        builder.Services.AddSingleton<IFOBControlService, FOBControlService>();
        return builder;
    }

    private static void AddFrameworkServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSignalR();
    }


    public static WebApplication UseFOBRequestPipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHub<ContestHub>("/client/contest");

        return app;
    }



}
