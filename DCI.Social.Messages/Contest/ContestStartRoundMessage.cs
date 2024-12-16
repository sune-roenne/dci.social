using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Contest;
public record ContestStartRoundMessage(
    long RoundExecutionId,
    int RoundIndex,
    string RoundName,
    string? Question,
    IReadOnlyCollection<ContestQuestionOption>? Options,
    bool IsBuzzerRound
    ) : AbstractMessage
{
    public const string MethodName = "ContestStartRound";
}

public record ContestQuestionOption(
    long OptionId,
    string OptionValue
    );
