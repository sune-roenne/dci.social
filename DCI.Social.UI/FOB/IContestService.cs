using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest.Definition;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.UI.Session;

namespace DCI.Social.UI.FOB;

public interface IContestService
{
    event EventHandler<RoundExecution> OnRoundBegin;
    event EventHandler<long> OnRoundEnd;
    Task Register(string user, string? userName);
    IReadOnlySet<string> RegisteredUsers();
    RoundExecution? CurrentRound { get; }
    string? CurrentRoundName { get; }
    IReadOnlyCollection<RoundOption>? CurrentRoundOptions { get; }
    int? CurrentRoundIndex { get; }
    string? CurrentQuestion { get; }
    bool CurrentIsBuzzer { get; }
    Task Buzz(string user, long roundExecutionId);
    Task SubmitAnswer(string user, long roundExecutionId, long optionId);

    event EventHandler<ContestRegistration> OnRegistrationAcknowledged;
    event EventHandler<IReadOnlySet<string>> OnNewBuzzAcknowledged;


}
