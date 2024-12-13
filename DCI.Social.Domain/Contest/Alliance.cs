using DCI.Social.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest;
public record Alliance(
    long AllianceId,
    SocialUser RequestedBy,
    SocialUser AcceptedBy,
    DateTime RequestTime,
    DateTime AcceptTime,
    DateTime? EndTime,
    SocialUser? EndedBy
    );
