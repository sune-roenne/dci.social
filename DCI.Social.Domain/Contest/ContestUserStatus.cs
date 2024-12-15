using DCI.Social.Domain.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest;
public record ContestUserStatus(
    SocialUser User,
    DateTime DrawnAt,
    int Points,
    ContestAllianceStatus? ActiveAlliance
    );
