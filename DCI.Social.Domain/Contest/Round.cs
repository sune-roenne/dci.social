using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest;
public record Round(
    long RoundId,
    long ContestId,
    string RoundName,
    TimeSpan RoundTime,
    int PointsNominal
    );
