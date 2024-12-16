using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters;
public interface IHeadQuartersService
{
    void UpdateUserMapping(IReadOnlyDictionary<string, SocialUser> userMap);
    Task<ExecutionStatus> NextRound();
    Task<ExecutionStatus> PreviousRound();
    Task<ExecutionStatus> StartRound();
    ExecutionStatus? CurrentStatus();

    bool HasPreviousRound();
    bool HasNextRound();

    Task<byte[]?> LoadSoundBytes(string soundId);

    Task ReloadState();

    Task<ExecutionStatus> MarkWinner(long roundExecutionId, long userId);

    event EventHandler<IReadOnlyCollection<Buzz>> OnBuzz;

    event EventHandler<IReadOnlyCollection<RoundScoring>> OnScorings;


}
