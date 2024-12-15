using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest;
public record ContestAllianceStatus(
    Alliance Alliance,
    int PointsByRequester,
    int PointsByAcceptor,
    DateTime DrawnAt
    )
{
    int Pot => PointsByRequester + PointsByAcceptor;

}
