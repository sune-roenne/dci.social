using DCI.Social.FOB.Common;
using DCI.Social.FOB.User;

namespace DCI.Social.FOB.Central;

internal class FOBControlService : IFOBControlService
{
    public Task StartContest(long contestId)
    {
        throw new NotImplementedException();
    }

    public Task<IdempotentActionComitted?> SubmitRoundOption(FOBUser user, long roundId, long optionId)
    {
        throw new NotImplementedException();
    }
}
