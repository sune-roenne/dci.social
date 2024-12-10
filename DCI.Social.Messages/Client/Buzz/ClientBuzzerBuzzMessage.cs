using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Client.Buzz;
public record ClientBuzzerBuzzMessage(
    string User
    ) : AbstractMessage
{
    public const string MethodName = "ClientBuzzerBuzz";
}
