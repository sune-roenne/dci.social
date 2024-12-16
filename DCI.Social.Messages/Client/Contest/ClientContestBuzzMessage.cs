using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Client.Contest;
public record ClientContestBuzzMessage(
    long RoundExecutionId,
    string User,
    DateTime RegistrationTime
    ) : AbstractMessage
{
    public const string MethodName = "ClientContestBuzz";


}
