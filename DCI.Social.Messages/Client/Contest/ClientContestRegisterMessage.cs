using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Client.Contest;
public record ClientContestRegisterMessage(
    string User,
    string? UserName
    ) : AbstractMessage
{
    public const string MethodName = "ClientContestRegister";
}
