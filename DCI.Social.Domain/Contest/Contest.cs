using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest;
public record Contest(
    long ContestId,
    string ContestName,
    IReadOnlyCollection<Round> Rounds
    )
{
    public readonly int TotalPointsNominal = Rounds.Sum(_ => _.PointsNominal);
}
