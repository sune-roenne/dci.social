using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Contest;
public record ContestAckRegisterMessage(
    long UserId,
    string User,
    string? UserName,
    DateTime RegistrationTime
    ) : AbstractMessage
{
    public const string MethodName = "ContestAckRegister";
}
