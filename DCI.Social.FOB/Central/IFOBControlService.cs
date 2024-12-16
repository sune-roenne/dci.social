using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.FOB.Common;
using DCI.Social.FOB.User;

namespace DCI.Social.FOB.Central;

public interface IFOBControlService
{

    Task StartContest(long contestId);

    Task<IdempotentActionComitted?> SubmitRoundOption(FOBUser user, long roundId, long optionId);
    Task StartBuzzerRound();

    Task HandleBuzz(Buzz buzz);
    Task AcknowledgeBuzz(Buzz buzz);

    Task RegisterUser(string user, string? userName);
    Task AckRegistration(long userId, string user, string? userName, DateTime registrationTime);

    Task DistributeRegistrations(IReadOnlyCollection<string> users);

    Task SubmitContestAnswer(long roundExecutionId, string user, long optionId);
    Task HandleContestBuzz(long roundExecutionId, string user);
    Task HandleContestAckBuzz(long roundExecutionId, string user, DateTime registrationTime);
    Task StartContestRound(long roundExecutionId, string roundName, bool isBuzzerRound, IReadOnlyCollection<RoundOption>? options, int roundIndex, string? question);
    Task EndContestRound(long roundExecutionId);


}
