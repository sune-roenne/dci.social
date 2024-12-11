using DCI.Social.Domain.Buzzer;
using DCI.Social.UI.Session;

namespace DCI.Social.UI.FOB;

public interface IFOBService
{
    Task Buzz(DCISocialUser user);

    event EventHandler<Buzz> OnBuzzAcknowledged;
    event EventHandler<string> OnBuzzerRoundStart;

}
