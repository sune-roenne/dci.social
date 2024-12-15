using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters;
public interface IHeadQuartersService
{

    Task<ExecutionStatus> StartContest(Contest contest);
    Task<ExecutionStatus> NextRound();
    Task<ExecutionStatus> PreviousRound();

    event EventHandler<IReadOnlyCollection<Buzz>> OnBuzz;

    event EventHandler<IReadOnlyCollection<RoundScoring>> OnScorings;


}
