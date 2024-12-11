using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Client.Buzz;
public record ClientBuzzerAckBuzzMessage(
    string User,
    string? UserName,
    DateTime RecordedTime
    ) : AbstractMessage
{
    public const string MethodName = "ClientBuzzerAckBuzz";
}
