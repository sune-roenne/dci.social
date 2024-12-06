using DCI.Social.FOB.Common;
using DCI.Social.FOB.User;

namespace DCI.Social.FOB.Controllers;

public interface IFOBControlService
{

    Task StartContest(long contestId);

    Task<IdempotentActionComitted?> SubmitRoundOption(FOBUser user, long roundId, long optionId);


}
