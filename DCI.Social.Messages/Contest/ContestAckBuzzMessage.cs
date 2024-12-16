using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Contest;
public record ContestAckBuzzMessage(
    long RoundExecutionId,
    string UserName,
    DateTime RegistrationTime
    ) : AbstractMessage
{
    public const string MethodName = "ContestAckMuzz";


}
