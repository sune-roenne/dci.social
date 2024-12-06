using DCI.Social.Messages.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Messages.Contest.Round;
public record RoundStartMessage(
    long RoundId,
    string RoundName,
    IReadOnlyCollection<RoundOptionDto> Options
    ) : AbstractMessage;
