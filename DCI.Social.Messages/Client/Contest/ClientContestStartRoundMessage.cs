using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Client.Contest;
public record ClientContestStartRoundMessage(
    long RoundExecutionId,
    int RoundIndex,
    string RoundName,
    string? Question,
    IReadOnlyCollection<ClientContestQuestionOption>? Options,
    bool IsBuzzerRound
    ) : AbstractMessage
{
    public const string MethodName = "ContestStartRound";
}

public record ClientContestQuestionOption(
    long OptionId,
    string OptionValue
    );
