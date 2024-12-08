using DCI.Social.UI.Server;

namespace DCI.Social.UI.Util;

public static class ConfigLogger {
   public static void LogConfiguration(this WebApplication app) 
   {
      var logger = app.Services.GetRequiredService<ILogger<App>>();
      var entries = app.Configuration
         .AsEnumerable()
         .OrderBy(_ => _.Key)
         .ToList(); 
      logger.LogInformation("App Configuration");  
      foreach(var conf in entries) {
        logger.LogInformation($"  {conf.Key}={conf.Value}");
      }    
   } 



}