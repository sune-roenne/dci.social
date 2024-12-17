using DCI.Social.FOB.Common;
using DCI.Social.FOB.User;
using DCI.Social.Messages.Client.Contest;
using DCI.Social.Messages.Contest;
using DCI.Social.Messages.Contest.Round;
using DCI.Social.Fortification.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using DCI.Social.Messages.Client.Buzz;
using DCI.Social.Domain.Buzzer;


namespace DCI.Social.FOB.Client;
//[Authorize(AuthenticationSchemes = FortificationAuthenticationConstants.ClientAuthenticationType)]
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

    public async Task ClientBuzzerBuzz(ClientBuzzerBuzzMessage mess) 
    {
        await WithControllerAction(async cont => {
            await cont.HandleBuzz(new Buzz(mess.User.ToLower(), mess.UserName, DateTime.Now));
        });
        await Clients.Caller.SendAsync(ClientBuzzerAckBuzzMessage.MethodName, new ClientBuzzerAckBuzzMessage(mess.User, mess.UserName, DateTime.Now));
    }

    public Task ClientContestRegister(ClientContestRegisterMessage message) => WithControllerService(async cont =>
    {
        Log($"Received client register message for {message.User}");
        await cont.RegisterUser(message.User, message.UserName);
        return 1;
    });

    public Task ClientContestBuzz(ClientContestBuzzMessage message) => WithControllerService(async cont =>
    {
        Log($"Received client BUZZ message for {message.User}");
        await cont.HandleContestBuzz(message.RoundExecutionId, message.User);
        return 1;
    });

    public Task ClientContestRegisterAnswer(ClientContestRegisterAnswerMessage message) => WithControllerService(async cont =>
    {
        Log($"Received client answer message for {message.User}");
        await cont.SubmitContestAnswer(message.RoundExecutionId, message.User, message.SelectedOptionId);
        return 1;
    });




}
