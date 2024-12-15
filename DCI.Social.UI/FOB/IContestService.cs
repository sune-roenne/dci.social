using DCI.Social.Domain.Buzzer;
using DCI.Social.Domain.Contest.Execution;
using DCI.Social.UI.Session;

namespace DCI.Social.UI.FOB;

public interface IContestService
{
    Task Register(string user, string? userName);
    IReadOnlySet<string> RegisteredUsers();

    event EventHandler<ContestRegistration> OnRegistrationAcknowledged;

}
