using DCI.Social.Domain.Buzzer;
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


}
