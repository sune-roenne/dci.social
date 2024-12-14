using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest.Definition;
public record QuestionRound(
    long RoundId,
    long ContestId,
    string RoundName,
    TimeSpan RoundTime,
    int PointsNominal,
    string? Question,
    long CorrectOptionId,
    IReadOnlyCollection<RoundOption>? RoundOptions
    ) : Round(RoundId, ContestId, RoundName, RoundTime, PointsNominal);
