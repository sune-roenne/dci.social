using DCI.Social.FOB.Common;
using DCI.Social.FOB.User;
using DCI.Social.Messages.Client.Contest;
using DCI.Social.Messages.Contest;
using DCI.Social.Messages.Contest.Round;
using Microsoft.AspNetCore.SignalR;

namespace DCI.Social.FOB.Client;

public class ClientHub : FOBHub
{

    private readonly ILogger<ClientHub> _logger;

    public ClientHub(ILogger<ClientHub> logger, IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
        _logger = logger;
    }

    public async Task SendRoundBeginMessage(RoundStartMessage message)
    {
        await Clients.All.SendAsync(RoundConstants.ClientMethodNames.BeginRound, message);
        _logger.LogInformation($"Sent begin round message: {message.RoundId} - {message.RoundName}");
    }

    public async Task SendRoundEndMessage(RoundEndMessage message)
    {
        await Clients.All.SendAsync(RoundConstants.ClientMethodNames.EndRound, message);
        _logger.LogInformation($"Sent end round message: {message.RoundId}");

    }

    public async Task SubmitRoundAnswer(RoundSubmitOptionMessage message)
    {
        var user = Context.User?.ExtractUser();
        if (user != null)
        {
            var acceptance = await WithControllerService(cont => cont.SubmitRoundOption(user, message.RoundId, message.OptionId));
            if (acceptance != null)
            {
                var retMes = new RoundConfirmSubmitOptionMessage(
                    message.RoundId, OptionId: message.OptionId,
                    AcceptedOnMessageId: acceptance.MessageId,
                    AcceptanceTime: acceptance.CommittedTime);
                await Clients.Caller.SendAsync(RoundConstants.ClientMethodNames.ConfirmOptionSubmitted);
            }
        }
    }

    public Task ClientContestRegister(ClientContestRegisterMessage message) => WithControllerService(async cont =>
    {
        await cont.RegisterUser(message.User, message.UserName);
        return 1;
    });



}
