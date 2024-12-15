using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCI.Social.Domain.Contest.Execution;
public record ContestRegistration(
    long UserId,
    string User,
    string? UserName
    );
