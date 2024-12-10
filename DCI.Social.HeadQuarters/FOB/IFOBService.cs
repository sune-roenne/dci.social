using DCI.Social.Domain.Buzzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.HeadQuarters.FOB;
public interface IFOBService
{
    Task InitBuzzerRound();
    Task ForwardBuzz(Buzz buzz);
    event EventHandler<Buzz> OnBuzz;

}
