using DCI.Social.Domain.Buzzer;

namespace DCI.Social.UI.FOB;

public interface IFOBService
{
    Task Buzz(string user);

    event EventHandler<Buzz> OnBuzzAcknowledged;
    event EventHandler<string> OnBuzzerRoundStart;

}
