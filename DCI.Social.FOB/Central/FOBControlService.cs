using DCI.Social.Domain.Buzzer;
using DCI.Social.FOB.Client;
using DCI.Social.FOB.Common;
using DCI.Social.FOB.HeadQuarters;
using DCI.Social.FOB.User;
using Microsoft.AspNetCore.SignalR;
using HQBuzzMess = DCI.Social.Messages.Contest.Buzzer.BuzzerBuzzMessage;
using ClientAckBuzzMess = DCI.Social.Messages.Client.Buzz.ClientBuzzerAckBuzzMessage;
using ClientStartRoundMess = DCI.Social.Messages.Client.Buzz.ClientBuzzerStartRoundMessage;
using DCI.Social.Messages.Contest;
using DCI.Social.Messages.Client.Contest;

namespace DCI.Social.FOB.Central;

internal class FOBControlService : IFOBControlService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public FOBControlService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task AcknowledgeBuzz(Buzz buzz) => await WithClientHub(async hub => await hub.Clients.All.SendAsync(ClientAckBuzzMess.MethodName, new ClientAckBuzzMess(buzz.User, buzz.UserName, buzz.BuzzTime)));

    public async Task HandleBuzz(Buzz buzz) => await WithHQHub(async hub => await hub.Clients.All.SendAsync(HQBuzzMess.MethodName, new HQBuzzMess(buzz.User, buzz.UserName, buzz.BuzzTime)));

    public async Task StartBuzzerRound() => await WithClientHub(async hub => await hub.Clients.All.SendAsync(ClientStartRoundMess.MethodName, new ClientStartRoundMess()));

    public Task StartContest(long contestId)
    {
        throw new NotImplementedException();
    }

    public Task<IdempotentActionComitted?> SubmitRoundOption(FOBUser user, long roundId, long optionId)
    {
        throw new NotImplementedException();
    }

    private async Task WithHQHub(Func<IHubContext<HeadQuartersHub>, Task> toPerform) => 
        await WithScope(async scope => await toPerform(scope.ServiceProvider.GetRequiredService<IHubContext<HeadQuartersHub>>()));
    private async Task WithClientHub(Func<IHubContext<ClientHub>, Task> toPerform) =>
        await WithScope(async scope => await toPerform(scope.ServiceProvider.GetRequiredService<IHubContext<ClientHub>>()));


    private async Task WithScope(Func<IServiceScope,Task> toPerform)
    {
        using var scope = _scopeFactory.CreateScope();
        await toPerform(scope);
    }

    public Task RegisterUser(string user, string? userName) => WithHQHub(async cont =>
    {
        await cont.Clients.All.SendAsync(ContestRegisterMessage.MethodName, new ContestRegisterMessage(User: user, UserName: userName));
    });

    public Task AckRegistration(long userId, string user, string? userName, DateTime registrationTime) => WithClientHub(async cont =>
    {
        await cont.Clients.All.SendAsync(ClientContestAckRegisterMessage.MethodName, new ClientContestAckRegisterMessage(userId, user, userName, registrationTime));
    });

    public Task DistributeRegistrations(IReadOnlyCollection<string> users) => WithClientHub(async cont =>
    {
        await cont.Clients.All.SendAsync(ClientContestRegisteredUsersMessage.MethodName, new ClientContestRegisteredUsersMessage(users));
    });
}
