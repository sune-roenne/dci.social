using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest.Definition;
public record Contest(
    long ContestId,
    string ContestName,
    IReadOnlyCollection<Round> Rounds
    )
{
    public readonly int TotalPointsNominal = Rounds.Sum(_ => _.PointsNominal);

    private Round[] _roundsArr => Rounds.ToArray();
    public bool HasRoundNo(int roundNo) => Rounds.Count > roundNo;

    public Round this[int roundNo] => _roundsArr[roundNo];


}
