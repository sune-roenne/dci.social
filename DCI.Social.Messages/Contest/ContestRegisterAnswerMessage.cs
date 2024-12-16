using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Contest;
public record ContestRegisterAnswerMessage(
    long RoundExecutionId,
    string User,
    long SelectedOptionId
    ) : AbstractMessage
{
    public const string MethodName = "ContestRegisterAnswer";


}

