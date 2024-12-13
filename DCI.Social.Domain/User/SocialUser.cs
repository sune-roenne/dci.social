using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.User;
public record SocialUser(
    long ExternalId,
    string Initials,
    string UserName
    );
