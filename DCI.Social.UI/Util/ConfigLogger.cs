using DCI.Social.UI.Server;
using DCI.Social.Domain.Util;

namespace DCI.Social.UI.Util;

public static class ConfigLogger {
   public static void LogConfiguration(this WebApplication app) 
   {
        var logger = app.Services.GetRequiredService<ILogger<App>>();
        var entriesString = app.Configuration
           .AsEnumerable()
           .OrderBy(_ => _.Key)
           .Select(_ => (_.Key, _.Value))
           .AsLoggableString(); 
        logger.LogInformation("App Configuration");
        logger.LogInformation(entriesString);

   } 



}