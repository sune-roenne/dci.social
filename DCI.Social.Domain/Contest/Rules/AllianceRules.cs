using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest.Rules;
public static class AllianceRules
{

    public static int TotalPointsToSplit(this ContestAllianceStatus status) => (status.PointsByRequester + status.PointsByAcceptor) switch
    {
        int sum when sum % 2 == 1 => sum + 1,
        int sum => sum
    };
    public static int PointsForEach(this ContestAllianceStatus status) => status.TotalPointsToSplit() / 2;



}
