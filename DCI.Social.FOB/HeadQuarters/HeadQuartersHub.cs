using DCI.Social.Domain.Buzzer;
using DCI.Social.FOB.Central;
using DCI.Social.FOB.Common;
using DCI.Social.Fortification.Authentication;
using DCI.Social.Messages.Contest;
using DCI.Social.Messages.Contest.Buzzer;
using Microsoft.AspNetCore.Authorization;

namespace DCI.Social.FOB.HeadQuarters;

[Authorize(AuthenticationSchemes = FortificationAuthenticationConstants.AuthenticationType)]
public class HeadQuartersHub : FOBHub
{
    private readonly IServiceScopeFactory _scopeFactory;
    public HeadQuartersHub(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task BuzzerStartRound(BuzzerStartRoundMessage mess)
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HeadQuartersHub>>();
        logger.LogInformation($"Received buzz start message: {mess.Message ?? ""}"); 
        var service = scope.ServiceProvider.GetRequiredService<IFOBControlService>();
        await service.StartBuzzerRound();
    }


    public async Task BuzzerAckBuzz(BuzzerAckBuzzMessage mess)
    {
        using var scope = _scopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<HeadQuartersHub>>();
        logger.LogInformation($"Received buzzer ack  message for: {mess.User}"); 
        var service = scope.ServiceProvider.GetRequiredService<IFOBControlService>();
        await service.AcknowledgeBuzz(new Buzz(mess.User, mess.UserName, DateTime.Now));
    }


    public async Task ContestAckRegister(ContestAckRegisterMessage mess) => await WithControllerService(async cont =>
    {
        await cont.AckRegistration(mess.UserId, mess.User, mess.UserName, mess.RegistrationTime);
        return 0;
    });

    public async Task ContestRegisteredUsers(ContestRegisteredUsersMessage mess) => await WithControllerService(async cont =>
    {
        await cont.DistributeRegistrations(mess.RegisteredUsers);
        return 0;
    });
}
