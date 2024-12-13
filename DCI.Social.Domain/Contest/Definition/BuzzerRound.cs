using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest.Definition;
public record BuzzerRound(
    long RoundId,
    long ContestId,
    string RoundName,
    TimeSpan RoundTime,
    int Points,
    Guid SoundId
    ) : Round(RoundId, ContestId, RoundName, RoundTime, Points);
