using DCI.Social.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest;
public record RoundScoring(
    long ScoringId,
    long RoundId,
    SocialUser ScoredBy,
    DateTime ScoreTime,
    int Points
    );
