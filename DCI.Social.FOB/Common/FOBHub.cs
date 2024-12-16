using DCI.Social.FOB.Central;
using Microsoft.AspNetCore.SignalR;

namespace DCI.Social.FOB.Common;

public class FOBHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FOBHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected async Task<T> WithControllerService<T>(Func<IFOBControlService, Task<T>> toPerform)
    {
        using var scope = _scopeFactory.CreateScope();
        var controller = scope.ServiceProvider.GetRequiredService<IFOBControlService>();
        var returnee = await toPerform(controller);
        return returnee;
    }

    protected async Task WithControllerAction(Func<IFOBControlService, Task> toPerform) =>
        _ = await WithControllerService(async controller =>
        {
            await toPerform(controller);
            return 0;
        });

    protected void Log(string mess) {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FOBHub>>();
        logger.LogInformation(mess);
    }    


}
