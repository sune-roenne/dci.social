using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Contest.Buzzer;
public record BuzzerBuzzMessage(
    string User
    ) : AbstractMessage;
